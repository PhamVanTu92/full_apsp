<template>
    <div class="flex justify-content-between align-content-center">
        <h4 class="font-bold m-0">{{ t('body.report.title_point_promotion') }}</h4>
        <div class="flex gap-2">
            <ButtonGoBack />
            <Button :label="t('body.report.export_excel_button_1')" outlined icon="pi pi-file-export" severity="info"
                @click="exportToExcel" :loading="loading.export" />
        </div>
    </div>
    <div class="flex gap-3 mt-3 mb-2">
        <span class="font-bold mt-2">{{ t('body.home.time_label') }}</span>
        <div>
            <Calendar v-model="query.FromDate" class="w-10rem" :placeholder="t('body.report.from_date_placeholder')"
                :maxDate="new Date()" :invalid="errMsg.FromDate ? true : false" />
            <small>{{ errMsg.FromDate }}</small>
        </div>
        <div>
            <Calendar v-model="query.ToDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')"
                :minDate="query.FromDate" :invalid="errMsg.ToDate ? true : false" />
            <small>{{ errMsg.ToDate }}</small>
        </div>
        <div class="w-30rem">
            <CustomerSelector v-model="query.cardId" selectionMode="multiple" optionValue="cardCode"
                :invalid="errMsg.CardCode ? true : false" />
            <small>{{ errMsg.CardCode }}</small>
        </div>
        <div>
            <Button @click="onClickConfirm" :label="t('body.report.apply_button_2')"/>
        </div>
    </div>
    <div class="card p-3">
        <DataTable :value="dataTable" v-model:expandedRows="expandedRows" dataKey="cardCode" paginator :rows="10"
            :rowsPerPageOptions="[5, 10, 20, 50]"
            paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown">
            <Column expander style="width: 3rem" />
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="cardCode" :header="t('body.report.table_header_customer_code_2')" />
            <Column :header="t('body.report.table_header_customer_name_3')" field="cardName" />
            <Column :header="t('body.report.table_header_received_points')" field="totalPoint" />
            <Column :header="t('body.report.table_header_used_points')" field="redeemPoint" />
            <Column :header="t('body.report.table_header_available_points')" field="remainingPoint" />

            <!-- Expand Row Template -->
            <template #expansion="slotProps">
                <div class="p-3">
                    <DataTable :value="slotProps.data.reportPoints" showGridlines
                        :paginator="slotProps.data.reportPoints?.length > 10" :rows="10"
                        :rowsPerPageOptions="[5, 10, 20, 50]" filterDisplay="menu" v-model:filters="detailFilters"
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

                        <Column :header="t('ChangePoint.changeType')" field="type" :showFilterMatchModes="false"
                            :filterMenuStyle="{ width: '14rem' }">
                            <template #filter="{ filterModel }">
                                <MultiSelect v-model="filterModel.value"
                                    :options="getTypeFilters(slotProps.data.reportPoints)" optionLabel="label"
                                    optionValue="value" :placeholder="t('ChangePoint.changeType')"
                                    class="p-column-filter" :maxSelectedLabels="1" />
                            </template>
                        </Column>

                        <Column :header="t('ChangePoint.pointChange')">
                            <template #body="{ data }">
                                <span :class="data.point < 0 ? 'text-red-500' : 'text-green-500'">
                                    {{ data.point < 0 ? data.point : "+" + data.point }} </span>
                            </template>
                        </Column>

                        <template #empty>
                            <div class="py-5 my-5 text-center text-500 font-italic">
                                {{ t('body.report.no_data_to_display_message_1') }}
                            </div>
                        </template>
                    </DataTable>
                </div>
            </template>

            <template #empty>
                <div class="py-5 my-5 text-center text-500 font-italic">
                    {{ t('body.report.no_data_to_display_message_1') }}
                </div>
            </template>
        </DataTable>
        <Loading v-if="loading.global" />
    </div>
</template>

<script setup lang="ts">
import { reactive, ref, watchEffect } from 'vue';
import API from '@/api/api-main';
import { format } from 'date-fns';
import { Validator, ValidateOption } from '@/helpers/validate';
import { useI18n } from 'vue-i18n';
import ExcelJS from 'exceljs';

const { t } = useI18n();
const currentDate = new Date();
const dataTable = ref([]);
const expandedRows = ref<any[]>([]);

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
    CardCode: {
        validators: {
            required: true,
            nullMessage: t('client.select_customer')
        }
    }
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

const detailFilters = ref<any>({
    type: { value: null, matchMode: 'in' }
});

const getTypeFilters = (reportPoints: any[]) => {
    if (!reportPoints || reportPoints.length === 0) return [];
    const uniqueTypes = [...new Set(reportPoints.map((item: any) => item.type))];
    return uniqueTypes.map(type => ({ label: type, value: type }));
};

const loading = reactive({
    global: false,
    export: false
});

const fetchData = (query: string) => {
    API.get(`Redeem/report?${query}`)
        .then((res) => {
            dataTable.value = res.data.items;
        })
        .catch()
        .finally(() => {
            loading.global = false;
        });
};

const isDate = (value: any): boolean => {
    return value instanceof Date;
};

// Hàm xuất Excel với ExcelJS - Có đầy đủ styling
const exportToExcel = async () => { 
    loading.export = true; 
    try {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('Báo cáo điểm thưởng'); 
        const addMergedCell = (range: string, value: string, style?: any) => {
            worksheet.mergeCells(range);
            const cell = worksheet.getCell(range.split(':')[0]);
            cell.value = value;
            if (style) Object.assign(cell, style);
        }; 
        const setCellStyle = (cell: any, style: any) => {
            Object.assign(cell, style);
        }; 
        const commonBorder = {
            top: { style: 'thin' },
            left: { style: 'thin' },
            bottom: { style: 'thin' },
            right: { style: 'thin' }
        }; 
        addMergedCell('A1:E1', 'Công Ty Cổ Phần AP Saigon Petro');
        addMergedCell('A2:E2', 'Văn phòng: Lầu 1, 6B Tôn Đức Thắng, Phường Sài Gòn, Thành phố Hồ Chí Minh, Việt Nam');
        addMergedCell('A5:F5', 'BÁO CÁO ĐIỂM THƯỞNG KHÁCH HÀNG', {
            font: { bold: true, size: 16 },
            alignment: { horizontal: 'center' }
        });  
        const logoPath = 'image/logo/apoil-logo.png';
        try {
            const logoResponse = await fetch(`/${logoPath}`);
            if (logoResponse.ok) {
                const logoArrayBuffer = await logoResponse.arrayBuffer();
                const logoImageId = workbook.addImage({
                    buffer: logoArrayBuffer,
                    extension: 'png',
                }); 
                addMergedCell('F1:F2', '', {
                    alignment: { horizontal: 'center', vertical: 'middle' }
                }); 
                worksheet.addImage(logoImageId, {
                    tl: { col: 5, row: 0 },
                    ext: { width: 80, height: 60 }
                });
            }
        } catch (error) {
            console.warn('Could not load logo:', error);
        }
        const dateRange = `Từ ngày ${format(query.FromDate, 'dd/MM/yyyy')} đến ngày ${format(query.ToDate, 'dd/MM/yyyy')}`;
        addMergedCell('A6:F6', dateRange, {
            font: { italic: true, size: 12 },
            alignment: { horizontal: 'center' }
        }); 
        worksheet.addRow([]);
        worksheet.addRow([]); 
        const headers = [
            '#',
            t('body.report.table_header_customer_code_2'),
            t('body.report.table_header_customer_name_3'),
            t('body.report.table_header_received_points'),
            t('body.report.table_header_used_points'),
            t('body.report.table_header_available_points')
        ]; 
        const headerRow = worksheet.addRow(headers);
        headerRow.eachCell(cell => setCellStyle(cell, {
            fill: { type: 'pattern', pattern: 'solid', fgColor: { argb: 'A6C9EC' } },
            font: { bold: true, color: { argb: 'FF000000' }, size: 11 },
            alignment: { horizontal: 'center', vertical: 'middle' },
            border: commonBorder
        })); 
        dataTable.value.forEach((customer: any, index: number) => { 
            const customerData = [
                index + 1,
                customer.cardCode,
                customer.cardName,
                customer.totalPoint,
                customer.redeemPoint,
                customer.remainingPoint
            ]; 
            const customerRow = worksheet.addRow(customerData);
            customerRow.eachCell(cell => setCellStyle(cell, {
                fill: { type: 'pattern', pattern: 'solid', fgColor: { argb: 'DAF2D0' } },
                font: { bold: false, color: { argb: 'FF000000' }, size: 11 },
                alignment: { horizontal: 'left', vertical: 'middle' },
                border: commonBorder
            })); 
            if (customer.reportPoints?.length) { 
                const detailHeaders = [
                    '',
                    t('body.report.table_header_order_code'),
                    t('body.home.time_label'),
                    t('ChangePoint.changeType'),
                    t('ChangePoint.pointChange'),
                    ''
                ]; 
                const detailHeaderRow = worksheet.addRow(detailHeaders);
                detailHeaderRow.eachCell(cell => setCellStyle(cell, {
                    font: { bold: true, color: { argb: 'FF000000' }, size: 10 },
                    border: commonBorder
                })); 
                customer.reportPoints.forEach((point: any) => {
                    const detailData = [
                        '',
                        point.invoiceCode,
                        format(point.docDate, 'dd/MM/yyyy'),
                        point.type,
                        point.point,
                        ''
                    ]; 
                    const detailRow = worksheet.addRow(detailData);
                    detailRow.eachCell((cell, colNumber) => {
                        setCellStyle(cell, {
                            alignment: { horizontal: 'left', vertical: 'middle' },
                            border: commonBorder,
                            ...(colNumber === 5 && {
                                font: { color: { argb: point.point < 0 ? 'FFFF0000' : 'FF00B050' } }
                            })
                        });
                    });
                });
            }
        }); 
        worksheet.columns = [
            { width: 8 }, { width: 20 }, { width: 35 },
            { width: 18 }, { width: 18 }, { width: 18 }
        ]; 
        const fileName = `Baocaodiemthuongkhachhang_${format(new Date(), 'ddMMyyyy')}.xlsx`;
        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], { 
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' 
        }); 
        const url = URL.createObjectURL(blob);
        const link = Object.assign(document.createElement('a'), {
            href: url,
            download: fileName
        }); 
        link.click();
        URL.revokeObjectURL(url);

    } catch (error) {
        console.error('Export error:', error);
        alert(t('body.report.export_failed'));
    } finally {
        loading.export = false;
    }
};

watchEffect(() => {
    fetchData(toQueryString());
});
</script>

<style scoped lang="css">
small {
    color: var(--red-500);
}

/* Style cho expand row */
:deep(.p-datatable-row-expansion) {
    background-color: var(--surface-50);
}

:deep(.p-datatable-row-expansion > td) {
    padding: 0 !important;
}

:deep(.p-datatable-row-expansion .p-3) {
    background-color: var(--surface-0);
}
</style>
