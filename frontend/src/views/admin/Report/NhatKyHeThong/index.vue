<template>
    <div>
        <div>
            <div class="flex justify-content-between align-content-center">
                <h4 class="font-bold m-0">{{ t('body.report.consentlog_report_title') }}</h4>
                <div class="flex gap-2">
                    <ButtonGoBack/>
                    <!-- <Button
                        label="In báo cáo"
                        outlined
                        icon="pi pi-print"
                        severity="warning"
                    /> -->
                    <!-- <Button label="Xuất excel" outlined icon="pi pi-file-export" severity="info"/> -->
                    <!-- <Button label="Gửi" outlined icon="pi pi-send"/> -->
                </div>
            </div>
            <div class="flex gap-3 mt-3">
                <span class="font-bold mt-2">{{ t('body.home.time_label') }}</span>
                <div>
                    <Calendar v-model="query.FromDate" class="w-10rem"
                        :placeholder="t('body.report.from_date_placeholder')" />
                </div>
                <div>
                    <Calendar v-model="query.ToDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')"
                        :minDate="query.FromDate" :maxDate="new Date()" />
                </div>
                <div class="w-30rem">
                    <InputText :placeholder="t('body.report.search_placeholder_3')" v-model="keySearch" class="w-full">
                    </InputText>
                </div>
                <div class="flex gap-1">
                    <Button @click="onClickConfirm" :label="t('body.report.apply_button_3')"/>
                    <Button v-if="keySearch" @click="() => {
                        keySearch = '';
                        page = 0;
                        fetchData();
                    }" severity="danger" outlined label="Xóa lọc" icon="pi pi-times">
                    </Button>
                </div>
            </div>

            <hr />
            <div class="card p-3"> 
                <DataTable :value="dataTable" showGridlines paginator :page="page" :rows="rows"
                    :totalRecords="totalRecords" lazy
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
                    @page="onPageChange"
                    :currentPageReportTemplate="`${t('body.productManagement.display')} {first} - {last} ${t('body.productManagement.total_of')} {totalRecords} ${t('body.systemSetting.orders')}`"
                    tableStyle="min-width: 124rem" selectionMode="single" scrollable scrollHeight="620px" :pt="{
                        rowgroupheader: {
                            class: 'surface-100'
                        }
                    }">
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column field="cardCode" :header="t('body.report.table_header_customer_code_2')"></Column>
                    <Column field="cardName" :header="t('body.report.table_header_customer_name_3')"></Column>
                    <Column field="orderCode" :header="t('body.report.table_header_order_code_2')"></Column>
                    <Column field="policyVersion" :header="t('body.report.table_header_policy_version')"></Column>
                    <Column field="agreeAt" :header="t('body.report.table_header_consent_time')">
                        <template #body="{ data }">
                            {{ format(data.agreeAt, 'dd/MM/yyyy') }}
                        </template>
                    </Column>
                    <Column field="ipAddress" :header="t('body.report.table_header_ip_address')"></Column>
                    <Column field="policyHash" :header="t('body.report.table_horder_volume_columneader_list_code')">
                    </Column>
                    <Column field="device" :header="t('body.report.table_header_device')"></Column>
                    <Column field="itemName" :header="t('body.systemSetting.note')"></Column>

                    <template #empty>
                        <div class="py-5 my-5 text-center text-500 font-italic">{{
                            t('body.report.no_data_to_display_message_1') }}</div>
                    </template>
                </DataTable>
            </div>
        </div>

        <!-- Chi tiết từng báo cáo  -->
        <Dialog v-model:visible="visibleDetail" header="Chi tiết báo cáo" modal>
            <div class="card p-3">
                <DataTable :value="detailTable" showGridlines tableStyle="min-width: 50rem">
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column header="Mã đơn hàng / yêu cầu lấy hàng gửi"></Column>
                    <Column header="Thời gian">
                        <template #body="{ data }">
                            <span>{{ format(data.docDate, 'dd/MM/yyyy') }}</span>
                        </template>
                    </Column>
                    <Column header="Số lượng tồn đầu kỳ" class="text-right"></Column>
                    <Column field="inQty" :header="t('body.report.table_header_imported_quantity')" class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column field="outQty" :header="t('body.report.table_header_exported_quantity')" class="text-right">
                        <template #body="{ data, field }">
                            {{ Intl.NumberFormat().format(data[field]) }}
                        </template>
                    </Column>
                    <Column header="Số lượng tồn kho cuối kỳ" class="text-right"></Column>
                    <template #empty>
                        <div class="py-5 my-5 text-center text-500 font-italic">{{
                            t('body.report.no_data_to_display_message_1') }}</div>
                    </template>
                </DataTable>
            </div>
        </Dialog>
        <Loading v-if="loading.global" />
    </div>
    <!--  Bộ lọc -->
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue';
import API from '@/api/api-main';
import { format } from 'date-fns';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const page = ref(0);
const rows = ref(10);
const totalRecords = ref(0);
const visibleDetail = ref(false);
const currentDate = new Date();
const keySearch = ref('');

onMounted(() => {
    fetchData();
});

const dataTable = ref([]);

const query = reactive({
    FromDate: new Date(`${currentDate.getFullYear()}-01-01`),
    ToDate: currentDate,
    CardCode: null as number[] | null
});

const onClickConfirm = () => {
    page.value = 0; // Reset về trang đầu khi apply filter
    loading.global = true;
    fetchData();
};

const toQueryString = (): string => {
    return Object.entries(query)
        .map(([key, value]) => (value ? `${key}=${isDate(value) ? format(value as Date, 'yyyy-MM-dd') : `,${Array(value).join(',')},`}` : null))
        .filter((item) => item)
        .join('&');
};

const detailTable = ref<any[]>([]);

const loading = reactive({
    global: false
});

const fetchData = () => {
    const queryString = toQueryString();
    let url = `PolicyOrderLog?Page=${page.value + 1}&PageSize=${rows.value}`;

    if (queryString) {
        url += `&${queryString}`;
    }

    if (keySearch.value) {
        url += `&search=${keySearch.value}`;
    }

    loading.global = true;

    API.get(url)
        .then((res) => {
            dataTable.value = [];
            if (res.data) {
                dataTable.value = res.data.items;
                totalRecords.value = res.data.total;
            }
        })
        .catch((err) => {
            console.error('Fetch error:', err);
        })
        .finally(() => {
            loading.global = false;
        });
};

const onPageChange = (e: any) => {
    page.value = e.page;
    rows.value = e.rows;
    fetchData();
};

const isDate = (value: any): boolean => {
    return value instanceof Date;
};
</script>
<style scoped lang="css">
small {
    color: var(--red-500);
}
</style>
