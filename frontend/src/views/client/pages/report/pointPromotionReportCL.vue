<template>
    <div>
        <div>
            <div class="flex justify-content-between align-content-center">
                <h4 class="font-bold m-0">{{ t('body.report.title_point_promotion') }}</h4>
                <div class="flex gap-2">
                    <ButtonGoBack />
                    <Button :label="t('body.report.export_excel_button_1')" outlined icon="pi pi-file-export"
                        severity="info" />
                </div>
            </div>
            <div class="flex gap-3 mt-3 mb-2">
                <span class="font-bold mt-2">{{ t('body.home.time_label') }}</span>
                <div>
                    <Calendar v-model="query.FromDate" class="w-10rem"
                        :placeholder="t('body.report.from_date_placeholder')" :maxDate="new Date()"
                        :invalid="errMsg.FromDate ? true : false" />
                    <small>{{ errMsg.FromDate }}</small>
                </div>
                <div>
                    <Calendar v-model="query.ToDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')"
                        :minDate="query.FromDate" :invalid="errMsg.ToDate ? true : false" />
                    <small>{{ errMsg.ToDate }}</small>
                </div>
                <div>
                    <Button @click="onClickConfirm" :label="t('body.report.apply_button_2')"/>
                </div>
            </div>
            <div class="card p-3">
                <DataTable :value="detailTable" showGridlines tableStyle="min-width: 50rem" paginator :rows="10"
                    :rowsPerPageOptions="[5, 10, 20, 50]"
                    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown">
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column :header="t('body.report.table_header_order_code')" field="invoiceCode" />
                    <Column :header="t('body.home.time_label')">
                        <template #body="{ data }">
                            <span>{{ format(data.docDate, 'dd/MM/yyyy') }}</span>
                        </template>
                    </Column>
                    <Column :header="t('ChangePoint.changeType')" field="type" />
                    <Column :header="t('ChangePoint.pointChange')">
                        <template #body="{ data }">
                            <span :class="data.point < 0 ? 'text-red-500' : 'text-green-500'">{{ data.point < 0 ?
                                data.point : "+" + data.point }}</span>
                        </template>
                    </Column>

                    <template #empty>
                        <div class="py-5 my-5 text-center text-500 font-italic">{{
                            t('body.report.no_data_to_display_message_1') }}</div>
                    </template>
                </DataTable>
            </div>
        </div>
        <Loading v-if="loading.global" />
    </div>
    <!--  Bộ lọc -->
</template>

<script setup lang="ts">
import { reactive, ref, watchEffect } from 'vue';
import API from '@/api/api-main';
import { format } from 'date-fns';
import { Validator, ValidateOption } from '@/helpers/validate';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const currentDate = new Date();
const errMsg = ref<any>({});
const validateOption: ValidateOption = {
    FromDate: {
        validators: {
            required: true,
            nullMessage: t('client.selectStartTime'),
            type: Date
        }
    },
    ToDate: {
        validators: {
            required: true,
            nullMessage: t('client.selectEndTime'),
            type: Date
        }
    },
};
const query = reactive({
    FromDate: new Date(`${currentDate.getFullYear()}-01-01`),
    ToDate: currentDate,
    cardId: null as number[] | null
});

const onClickConfirm = () => {
    const vResult = Validator(query, validateOption);
    errMsg.value = {};
    if (!vResult.result) {
        errMsg.value = vResult.errors;
        return;
    }
    loading.global = true;
    fetchData(toQueryString());
};

const toQueryString = (): string => {
    return Object.entries(query)
        .map(([key, value]) => {
            if (!value) return null;

            let formattedValue: string;
            if (isDate(value)) {
                formattedValue = format(value as Date, 'yyyy-MM-dd');
            } else {
                const arrayValue = new Array(value).join(',');
                formattedValue = `,${arrayValue},`;
            }

            return `${key}=${formattedValue}`;
        })
        .filter(Boolean)
        .join('&');
};

const detailTable = ref<any[]>([]);

const loading = reactive({
    global: false
});
const fetchData = (query: string) => {
    API.get(`Redeem/report?${query}`)
        .then((res) => {
            detailTable.value = res.data.items[0].reportPoints;
        })
        .catch()
        .finally(() => {
            loading.global = false;
        });
};

const isDate = (value: any): boolean => {
    return value instanceof Date;
};

watchEffect(() => {
    fetchData(toQueryString());
});
</script>
<style scoped lang="css">
small {
    color: var(--red-500);
}
</style>
