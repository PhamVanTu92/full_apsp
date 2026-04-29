<template>
    <div>
        <div class="flex justify-content-between align-content-center mb-3">
            <h4 class="font-bold m-0">{{ t('body.report.report_title_customer_debt') }}</h4>
            <ButtonGoBack />
        </div>
        <div class="grid">
            <div class="col-7">
                <div class="grid">
                    <div class="col-5">
                        <div class="field formgrid">
                            <label for="firstname3" style="width: 100px">{{ t('body.report.area_label') }}</label>
                            <div class="">
                                <MultiSelect v-model="query.location" :options="options.location" optionLabel="name" optionValue="name" class="w-full" filter :placeholder="t('body.report.area_placeholder')"></MultiSelect>
                            </div>
                        </div>
                    </div>
                    <div class="col-7">
                        <div class="field formgrid">
                            <label for="firstname3" style="width: 170px">{{ t('body.report.responsible_staff_label') }}</label>
                            <div class="">
                                <Dropdown v-model="query.Employee" :options="options.user" optionLabel="name" optionValue="id" class="w-full" filter :placeholder="t('body.report.responsible_staff_placeholder')"></Dropdown>
                            </div>
                        </div>
                    </div>
                    <div class="col-5 pt-0">
                        <div class="field formgrid">
                            <label for="firstname3" style="width: 100px">{{ t('body.home.time_label') }}</label>
                            <div class="my-auto py-2">
                                <span>
                                    {{ format(query.ToDate || '', 'dd/MM/yyyy') }}
                                </span>
                            </div>
                        </div>
                    </div>
                    <div class="col-7 pt-0">
                        <div class="field formgrid">
                            <label for="firstname3" style="width: 170px">{{ t('body.report.customer_label_1') }}</label>
                            <div class="">
                                <CustomerSelector v-model="query.CardCode" selectionMode="multiple" :placeholder="t('body.report.area_placeholder')"></CustomerSelector>
                            </div>
                        </div>
                    </div>
                    <div class="col-12 flex py-0 justify-content-end">
                        <Button @click="onClickConfirm" :label="t('body.report.apply_button_2')" :loading="loading" />
                    </div>
                </div>
            </div>
            <!-- ---------------------------------------- -->
            <div class="col-5 flex gap-3 justify-content-end">
                <div class="flex-grow-1">
                    <div class="card p-3 h-full flex flex-column border-200">
                        <div class="font-bold text-lg">{{ t('body.report.credit_debt_title') }}</div>
                        <div class="text-right text-xl flex-grow-1 flex flex-column justify-content-center">
                            <!-- {{ dataTable.footer.overCredit }} -->
                            {{ dataTable.footer.totalCredit }}
                        </div>
                    </div>
                </div>
                <div class="flex-grow-1">
                    <div class="card p-3 h-full flex flex-column border-200">
                        <div class="font-bold text-lg">{{ t('body.report.guaranteed_debt_title') }}</div>
                        <div class="text-right text-xl flex-grow-1 flex flex-column justify-content-center">
                            <!-- {{ dataTable.footer.overGuarantee }} -->
                            {{ dataTable.footer.totalGuarantee }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <hr />
        <DataTable :value="dataTable.value" showGridlines stripedRows resizableColumns columnResizeMode="expand" @row-click="onRowClick" selectionMode="single" class="card p-3" scrollable>
            <ColumnGroup type="header">
                <Row>
                    <Column :header="t('body.report.table_header_stt_4')" :rowspan="2"></Column>
                    <Column :header="t('body.report.table_header_customer_code_1')" :rowspan="2"></Column>
                    <Column :header="t('body.report.table_header_customer_name_2')" :rowspan="2"></Column>
                    <Column :header="t('body.report.table_header_debt_limit')" :colspan="2"></Column>
                    <Column :header="t('body.report.table_header_actual_debt')" :colspan="2"></Column>
                    <Column :header="t('body.report.table_header_overdue_amount')" :colspan="2"></Column>
                    <!-- <Column header="Hạn thanh toán" :colspan="2"></Column> -->
                    <!-- <Column header="" frozen :rowspan="2"></Column> -->
                </Row>
                <Row>
                    <template v-for="i in Array(3)" :key="i">
                        <Column :header="t('body.report.table_header_credit_debt_limit') "></Column>
                        <Column :header="t('body.report.table_header_guaranteed_debt_limit') "></Column>
                    </template>
                    <!--
                        <Column header="Nợ tín chấp (Ngày)"></Column>
                        <Column header="Nợ bảo lãnh (Ngày)"></Column>
                    -->
                </Row>
            </ColumnGroup>

            <Column header="#">
                <template #body="sp">
                    {{ sp.index + 1 }}
                </template>
            </Column>
            <Column field="cardCode"></Column>
            <Column field="cardName"></Column>
            <!-- number -->
            <Column field="credit" body-class="text-right"
                ><template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template></Column
            >
            <Column field="guarantee" body-class="text-right"
                ><template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template></Column
            >
            <Column field="totalCredit" body-class="text-right">
                <template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template>
            </Column>
            <Column field="totalGuarantee" body-class="text-right"
                ><template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template></Column
            >
            <Column field="overCredit" body-class="text-right"
                ><template #body="{ data, field }">
                    <span v-if="data[field] < 0" class="text-red-500">
                        {{ fnum(Math.abs(data[field])) }}
                    </span>
                    <span v-else>-</span></template
                ></Column
            >
            <Column field="overGuarantee" body-class="text-right"
                ><template #body="{ data, field }">
                    <span v-if="data[field] < 0" class="text-red-500">
                        {{ fnum(Math.abs(data[field])) }}
                    </span>
                    <span v-else>-</span>
                </template></Column
            >
            <!-- <Column field="timeCredit" body-class="text-right"
                ><template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template></Column
            >
            <Column field="timeGuarantee" body-class="text-right"
                ><template #body="{ data, field }">
                    {{ fnum(data[field]) }}
                </template></Column
            > -->
            <!-- number -->
            <template #empty>
                <p class="py-5 my-5 text-center text-muted">{{ t('body.report.no_data_to_display_message_1') }}</p>
            </template>

            <ColumnGroup type="footer">
                <Row>
                    <Column :footer="t('body.home.total')" :colspan="5" footerStyle="text-align:left" />
                    <Column :footer="dataTable.footer.totalCredit" footerStyle="text-align:right" />
                    <Column :footer="dataTable.footer.totalGuarantee" footerStyle="text-align:right" />
                    <Column :footer="dataTable.footer.overCredit" footerStyle="text-align:right" />
                    <Column :footer="dataTable.footer.overGuarantee" footerStyle="text-align:right" />
                    <!-- <Column footerStyle="text-align:right" />
                    <Column footerStyle="text-align:right" /> -->
                </Row>
            </ColumnGroup>
        </DataTable>
        <APRDialog ref="dialog" />
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { format } from 'date-fns';
import apiService from '@/api/api-main';
import APRDialog from './APRDialog.vue';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const dialog = ref<InstanceType<typeof APRDialog>>();
const query = reactive<Partial<{ ToDate: Date; location: any; Employee: any; CardCode: any[] }>>({
    ToDate: new Date(),
    location: null,
    Employee: null
    // CardCode: []
});
const dataTable = reactive({
    value: [],
    footer: {
        totalCredit: '',
        totalGuarantee: '',
        overCredit: '',
        overGuarantee: ''
    }
});

const toQueryString = () => {
    const params = new URLSearchParams();
    for (const key in query) {
        const _key = key as keyof typeof query;
        if (query[_key] !== null && query[_key] !== undefined) {
            if (_key == 'ToDate') {
                params.append(key, format(query[_key], 'yyyy-MM-dd'));
                // } else if (_key == 'CardCode' && query[_key]) {
                //     params.append(key, `,${query[_key]?.toString()},`);
            } else if (query[_key] != null) {
                params.append(key, `,${query[_key]},`);
            }
        }
    }
    return params.toString();
};
const loading = ref(false);
const toast = useToast();
const onClickConfirm = () => {
    loading.value = true;
    Object.keys(dataTable.footer).forEach((key) => {
        // Reset footer values
        dataTable.footer[key as keyof typeof dataTable.footer] = '';
    });
    apiService
        .get(`Report/debtReport?${toQueryString()}`)
        .then((res) => {
            if (!res.data) {
                toast.add({
                    severity: 'success',
                    summary: 'Thông báo',
                    detail: t('body.report.no_data_message_1'),
                    life: 3000
                });
                dataTable.value = [];
                return;
            }
            dataTable.value = res.data;
            dataTable.footer.totalCredit = fnum(sumCol(res.data, 'totalCredit'));
            dataTable.footer.totalGuarantee = fnum(sumCol(res.data, 'totalGuarantee'));
            dataTable.footer.overCredit =
                fnum(
                    Math.abs(
                        sumCol(
                            res.data.filter((row: any) => row.overCredit < 0),
                            'overCredit'
                        )
                    )
                ) + ' VND';
            dataTable.footer.overGuarantee =
                fnum(
                    Math.abs(
                        sumCol(
                            res.data.filter((row: any) => row.overGuarantee < 0),
                            'overGuarantee'
                        )
                    )
                ) + ' VND';
        })
        .catch((error) => {
            toast.add({
                severity: 'error',
                summary: 'Lỗi',
                detail: 'Không thể tải dữ liệu báo cáo. Vui lòng thử lại sau.',
                life: 3000
            });
        })
        .finally(() => {
            loading.value = false;
        });
};

const selection = reactive({
    location: null,
    user: 0
});

const options = reactive({
    location: [],
    user: [
        {
            id: 0,
            name: t('body.report.area_placeholder')
        }
    ]
});

const onSearchUser = (event: any) => {

};

const onRowClick = (event: any) => {
    dialog.value?.open({
        tableValue: event.data.bccnDetail,
        cardCode: event.data.cardCode,
        cardName: event.data.cardName
    });
};

const fetchLocation = () => {
    apiService.get('regions').then((res) => {
        options.location = res.data?.items;
    });
};

const fnum = (num: number) => {
    if (!num) return '0';
    return Intl.NumberFormat().format(num);
};

const sumCol = (array: any[], field: string) => {
    return array.reduce((acc, item) => {
        if (item[field] !== null && item[field] !== undefined) {
            return acc + item[field];
        }
        return acc;
    }, 0);
};

onMounted(() => {
    fetchLocation();
});
</script>
<style scoped>
.col-5,
.col-7 {
    padding-bottom: 0;
}
</style>
