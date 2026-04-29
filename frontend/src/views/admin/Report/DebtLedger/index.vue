<template>
    <div>
        <div class="mb-3 flex justify-content-between">
            <h4 class="m-0 font-bold">{{ t('body.report.report_title_debt_details_by_object') }}</h4>
            <div>
                <Button @click="exportExcel()" icon="pi pi-file-export" :label="t('body.report.export_excel_button_3')" severity="info" class="mr-3" />
                <ButtonGoBack />
            </div>
        </div>
        <div class="grid">
            <div class="col-6">
                <div class="form-control">
                    <div>
                        <label>{{ t('body.report.customer_label_1') }}</label>
                        <CustomerSelector v-model="filter.CardCode" class="w-30rem" optionValue="cardCode" :invalid="!!errMsg.CardCode" />
                        <small>{{ errMsg.CardCode }}</small>
                    </div>
                    <div class="flex gap-3">
                        <div class="flex-grow-1">
                            <label>{{ t('body.report.from_date_label_2') }}</label>
                            <Calendar v-model="filter.FromDate" class="w-full" :maxDate="filter.ToDate" :invalid="!!errMsg.FromDate"></Calendar>
                            <div>
                                <small>{{ errMsg.FromDate }}</small>
                            </div>
                        </div>
                        <div class="flex-grow-1">
                            <label>{{ t('body.report.to_date_label_2') }}</label>
                            <Calendar v-model="filter.ToDate" class="w-full" :minDate="filter.FromDate" :invalid="!!errMsg.ToDate"></Calendar>
                            <div class="">
                                <small>{{ errMsg.ToDate }}</small>
                            </div>
                        </div>
                        <div class="pt-4">
                            <Button @click="onClickConfirmFilter" :label="t('body.report.apply_button_2')" :disabled="loading" />
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-6">
                <DataTable class="h-full" :value="DATA.statsDataTable" showGridlines>
                    <Column field="header" :header="t('body.report.document_title')"></Column>
                    <Column field="value1" :header="t('body.report.debit_label')" class="w-10rem text-right"></Column>
                    <Column field="value2" :header="t('body.report.credit_label')" class="w-10rem text-right"></Column>
                </DataTable>
            </div>
        </div>
        <hr class="mt-0 mb-5" />
        <DataTable :value="DATA.dataTable" class="card p-3" showGridlines stripedRows scrollable :scrollHeight="`${height}px`" :loading="loading">
            <ColumnGroup type="header">
                <Row>
                    <Column :header="t('body.report.table_header_stt_4')" :rowspan="2"></Column>
                    <Column :header="t('body.report.document_date')" :rowspan="2"></Column>
                    <Column :header="t('body.report.document_title')" :colspan="2"></Column>
                    <Column :header="t('body.report.explanation_label')" :rowspan="2"></Column>
                    <Column :header="t('body.report.debit_label')" :colspan="2"></Column>
                    <Column :header="t('body.report.invoice_number')" :rowspan="2"></Column>
                    <Column :header="t('body.report.exchange_rate')" :rowspan="2"></Column>
                </Row>
                <Row>
                    <Column :header="t('body.report.document_number')"></Column>
                    <Column :header="t('body.report.document_date')"></Column>
                    <Column :header="t('body.report.debit_label')"></Column>
                    <Column :header="t('body.report.credit_label')"></Column>
                </Row>
            </ColumnGroup>

            <Column header="#" class="w-1rem text-right">
                <template #body="{ index }">
                    <span>{{ index + 1 }}</span>
                </template>
            </Column>
            <Column :header="t('body.report.document_date')" class="w-9rem">
                <template #body="{ data }">
                    {{ data.refDate ? format(data.refDate, dateFormat) : '' }}
                </template>
            </Column>
            <Column :header="t('body.report.document_number')" class="w-9rem">
                <template #body="{ data }">
                    <span class="font-bold">{{ data.voucherNo }}</span>
                </template>
            </Column>
            <Column :header="t('body.report.document_date')" class="w-9rem">
                <template #body="{ data }">
                    {{ data.taxDate ? format(data.taxDate, dateFormat) : '' }}
                </template>
            </Column>
            <Column :header="t('body.report.explanation_label')">
                <template #body="{ data }">
                    {{ data.lineMemo }}
                </template>
            </Column>
            <Column :header="t('body.report.debit_label')" class="text-right">
                <template #body="{ data }">
                    {{ fnum(data.debit || 0) }}
                </template>
            </Column>
            <Column :header="t('body.report.credit_label')" class="text-right">
                <template #body="{ data }">
                    {{ fnum(data.credit || 0) }}
                </template>
            </Column>
            <Column :header="t('body.report.invoice_number')">
                <template #body="{ data }">
                    <span class="font-bold text-primary">{{ data.lineMemo }}</span>
                </template>
            </Column>
            <Column :header="t('body.report.exchange_rate')" class="text-right">
                <template #body="{ data }">{{ data.rate }}</template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :colspan="5" :footer="t('body.report.total_transactions_in_period')"></Column>
                    <Column :footer="tspstk_no" class="text-right"></Column>
                    <Column :footer="tspstk_co" class="text-right"></Column>
                    <Column></Column>
                    <Column></Column>
                </Row>
                <Row>
                    <Column :colspan="5" :footer="t('body.report.ending_balance')"></Column>
                    <Column :footer="sdck_no" class="text-right"></Column>
                    <Column :footer="sdck_co" class="text-right"></Column>
                    <Column></Column>
                    <Column></Column>
                </Row>
            </ColumnGroup>
            <template #header>
                <div class="flex justify-content-between align-items-center">
                    <span
                        >{{ t('body.report.total_rows_label') }} <span>{{ DATA.dataTable?.length }}</span></span
                    >
                    <div class="flex align-items-center gap-5">
                        <Slider :min="0" :max="2000" v-model="height" class="w-20rem hidden"></Slider>
                        <!-- <InputText placeholder="Tìm kiếm..."></InputText> -->
                    </div>
                </div>
            </template>
            <template #empty>
                <div class="text-center text-500 my-5 py-5 font-italic">{{ t('body.report.no_data_to_display_message_1') }}</div>
            </template>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from 'vue';
import { format } from 'date-fns';
import API from '@/api/api-main';
import { Validator, ValidateOption } from '@/helpers/validate';
import { useToast } from 'primevue/usetoast';
import { useI18n } from 'vue-i18n';
import ExcelJS from 'exceljs';
const { t } = useI18n();

const dateFormat = 'dd/MM/yyyy';
const filter = reactive({
    CardCode: null,
    FromDate: new Date(new Date().getFullYear(), 0, 1),
    ToDate: new Date(),
    search: '',
    cardName: ''
});
const toast = useToast();

const toQueryString = (): string => {
    return `?FromDate=${filter.FromDate ? format(filter.FromDate, 'yyyy-MM-dd') : ''}&ToDate=${filter.ToDate ? format(filter.ToDate, 'yyyy-MM-dd') : ''}&CardCode=${filter.CardCode}`;
};
const optVld: ValidateOption = {
    CardCode: {
        validators: {
            required: true,
            nullMessage: 'Vui lòng chọn khách hàng'
        }
    },
    FromDate: {
        validators: {
            required: true,
            nullMessage: 'Vui lòng chọn thời gian'
        }
    },
    ToDate: {
        validators: {
            required: true,
            nullMessage: 'Vui lòng chọn thời gian'
        }
    }
};
const errMsg = ref<any>({});
const onClickConfirmFilter = () => {
    const vResult = Validator(filter, optVld);
    errMsg.value = vResult.errors;
    if (vResult.result) {
        fetchData();
    } else {
        DATA.dataTable = [];
    }
};

const height = ref(700);

const DATA = reactive({
    dataTable: [] as any,
    statsDataTable: [
        {
            header: 'Số dư đầu kỳ',
            value1: '0',
            value2: '0'
        },
        {
            header: 'Số dư phát sinh trong kì',
            value1: '0',
            value2: '0'
        }
    ]
});
const tspstk_no = ref('');
const tspstk_co = ref('');
const sdck_no = ref('');
const sdck_co = ref('');

const fnum = (input: number): string => {
    if (!input) return '0';
    return Intl.NumberFormat().format(input);
};
const loading = ref(false);
const fetchData = () => {
    loading.value = true;
    DATA.dataTable = [];
    API.get(`Report/BalanceBPReport${toQueryString()}`)
        .then((res) => {
            if (res.data) {
                DATA.dataTable = res.data[0]?.detail;
                DATA.statsDataTable[0].value1 = fnum(res.data[0]?.obDebit || 0);
                DATA.statsDataTable[0].value2 = fnum(res.data[0]?.obCredit || 0);
                DATA.statsDataTable[1].value1 = fnum(res.data[0]?.debit || 0);
                DATA.statsDataTable[1].value2 = fnum(res.data[0]?.credit || 0);
                filter.cardName = res.data[0]?.cardName || '';
                tspstk_no.value = fnum(res.data[0]?.debit || 0);
                tspstk_co.value = fnum(res.data[0]?.credit || 0);
                sdck_no.value = fnum(res.data[0]?.ebDebit || 0);
                sdck_co.value = fnum(res.data[0]?.ebCredit || 0);
            } else {
                toast.add({
                    severity: 'warn',
                    summary: 'Thông báo',
                    detail: 'Khách hàng này chưa phát sinh giao dịch',
                    life: 3000
                });
            }
        })
        .catch()
        .finally(() => {
            loading.value = false;
        });
};

// watch(
//     () => ({
//         CardCode: filter.CardCode,
//         FromDate: filter.FromDate,
//         ToDate: filter.ToDate,
//     }),
//     (value) => {
//         if (value.CardCode && value.FromDate && value.ToDate) {
//             fetchData();
//         } else {
//             DATA.dataTable = [];
//         }
//     }
// );

function randomNumber(a: number, b: number) {
    // Đảm bảo a luôn nhỏ hơn hoặc bằng b
    if (a > b) {
        // Hoán đổi giá trị a và b nếu a > b
        [a, b] = [b, a];
    }

    // Tạo số ngẫu nhiên từ a đến b (bao gồm cả a và b)
    return Math.floor(Math.random() * (b - a + 1)) + a;
}
const exportExcel = async () => { 
    if (!filter?.CardCode) {
        toast.add({
            severity: 'warn',
            summary: 'Thông báo',
            detail: 'Vui lòng chọn khách hàng',
            life: 3000
        });
        return;
    }

    if (!DATA?.dataTable?.length) {
        toast.add({
            severity: 'error',
            summary: 'Thông báo',
            detail: 'Không có dữ liệu để xuất Excel',
            life: 3000
        });
        return;
    }

    try { 
        const parseNumber = (value: any): number => {
            if (value === null || value === undefined) return 0;
            const str = String(value).replace(/[^0-9.-]/g, '');
            const num = Number(str);
            return isNaN(num) ? 0 : num;
        }; 
        const formatDate = (date: any): string => {
            try {
                return date ? format(date, dateFormat) : '';
            } catch {
                return '';
            }
        }; 
        const workbook = new ExcelJS.Workbook();
        workbook.creator = 'PetroApp';
        workbook.created = new Date();
        const worksheet = workbook.addWorksheet('Sổ chi tiết công nợ'); 
        worksheet.addRow(['SỔ CHI TIẾT CÔNG NỢ THEO ĐỐI TƯỢNG']);
        worksheet.addRow([`Từ ngày ${formatDate(filter.FromDate)} đến ngày ${formatDate(filter.ToDate)}`]);
        worksheet.addRow([`Đối tượng : ${filter.CardCode}${filter.cardName ? ' - ' + filter.cardName : ''}`]);
        worksheet.addRow([]); 
        worksheet.addRow(['STT', 'Ngày ghi sổ', 'Chứng từ', '', 'Diễn giải', 'Số tiền', '', 'Số HĐ', 'Tỷ giá']);
        worksheet.addRow(['', '', 'Số hiệu', 'Ngày tháng', '', 'Nợ', 'Có', '', '']); 
        const statsData = DATA?.statsDataTable || [];
        const obDebit = parseNumber(statsData[0]?.value1);
        const obCredit = parseNumber(statsData[0]?.value2);
        const periodDebit = parseNumber(statsData[1]?.value1);
        const periodCredit = parseNumber(statsData[1]?.value2); 
        const addFormattedRow = (label: string, debit: number, credit: number) => {
            const row = worksheet.addRow(['', '', '', '', label, debit, credit]);
            [6, 7].forEach((col) => {
                const cell = row.getCell(col);
                cell.numFmt = '#,##0';
                cell.alignment = { horizontal: 'right', vertical: 'middle' };
            });
            return row;
        }; 
        addFormattedRow('Số dư đầu kỳ', obDebit, obCredit); 
        addFormattedRow('Số phát sinh trong kì', periodDebit, periodCredit); 
        DATA.dataTable.forEach((item: any, index: number) => {
            const rowValues = [ 
                index + 1,
                formatDate(item?.refDate),
                item?.voucherNo || '',
                formatDate(item?.taxDate),
                item?.lineMemo || '',
                parseNumber(item?.debit),
                parseNumber(item?.credit),
                item?.invoiceNo || item?.lineMemo || '',
                parseNumber(item?.rate)
            ];
            worksheet.addRow(rowValues);
        }); 
        const totDebit = parseNumber(tspstk_no?.value);
        const totCredit = parseNumber(tspstk_co?.value);
        const endDebit = parseNumber(sdck_no?.value);
        const endCredit = parseNumber(sdck_co?.value); 
        addFormattedRow('Tổng số phát sinh trong kỳ', totDebit, totCredit);
        addFormattedRow('Số dư cuối kỳ', endDebit, endCredit); 
        const merges = ['A1:I1', 'A2:I2', 'A3:I3', 'A4:I4', 'A5:A6', 'B5:B6', 'E5:E6', 'H5:H6', 'I5:I6', 'C5:D5', 'F5:G5'];
        merges.forEach((range) => worksheet.mergeCells(range));
        worksheet.columns = [{ width: 6 }, { width: 14 }, { width: 14 }, { width: 14 }, { width: 50 }, { width: 16 }, { width: 16 }, { width: 18 }, { width: 12 }];
 
        for (let r = 1; r <= 6; r++) {
            const row = worksheet.getRow(r);
            row.height = r <= 3 ? 20 : 16;
            row.eachCell((cell) => {
                cell.alignment = {
                    horizontal: 'center',
                    vertical: 'middle',
                    wrapText: true
                };
                cell.font = {
                    name: 'Arial',
                    size: r === 1 || r === 3 ? 14 : 11,
                    bold: r === 1 || r === 3
                };
            });
        }
 
        const lastRow = worksheet.rowCount;
        for (let r = 7; r <= lastRow; r++) {
            const row = worksheet.getRow(r);
            [6, 7].forEach((colNum) => {
                const cell = row.getCell(colNum);
                if (cell.value !== null && cell.value !== undefined) {
                    cell.numFmt = '#,##0';
                    cell.alignment = { horizontal: 'right', vertical: 'middle' };
                }
            });
        }
 
        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a'); 
        link.href = url;
        link.download = `Sochitietcongnotheodoituong_${format(new Date(), 'yyyyMMdd')}_${filter.CardCode}.xlsx`;
        document.body.appendChild(link);
        link.click(); 
        setTimeout(() => {
            document.body.removeChild(link);
            URL.revokeObjectURL(url);
        }, 100); 
        toast.add({
            severity: 'success',
            summary: 'Thành công',
            detail: 'Xuất file Excel thành công',
            life: 3000
        });
    } catch (error) {
        console.error('Export Excel error:', error);
        toast.add({
            severity: 'error',
            summary: 'Lỗi',
            detail: error instanceof Error ? error.message : 'Không thể xuất file Excel',
            life: 4000
        });
    }
};
const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
small {
    color: var(--red-500);
}
.form-control > * {
    margin-bottom: 1rem;
}
.form-control label {
    margin-bottom: 4px !important;
    display: block;
}
</style>
