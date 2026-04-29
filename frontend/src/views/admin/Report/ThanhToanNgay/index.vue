<template>
    <div>
        <div class="flex justify-content-between align-content-center">
            <h4 class="font-bold m-0">{{ t('body.report.report_title_immediate_payment_bonus') }}</h4>
            <div class="flex gap-2">
                <ButtonGoBack></ButtonGoBack>
                <Button :label="t('body.report.export_excel_button_1')" outlined icon="pi pi-file-export"
                    severity="info" @click="exportToExcel" :loading="loading.export">
                </Button>
            </div>
        </div> 
        <div class="flex align-items-center gap-5 mt-2">
            <div class="flex align-items-center gap-3">
                <span>{{ t('body.report.customer_label_1') }}</span>
                <CustomerSelector v-model="queryParam.CardId" class="w-30rem" selectionMode="multiple">
                </CustomerSelector>
            </div>
            <div class="flex align-items-center gap-3">
                <span>{{ t('body.report.time_label_3') }}</span>
                <Calendar v-model="queryParam.fromDate" :placeholder="t('body.report.from_date_placeholder')"
                    class="w-10rem" :maxDate="queryParam.toDate" :invalid="!!errMsg.fromDate"></Calendar>
                <Calendar v-model="queryParam.toDate" :placeholder="t('body.report.to_date_placeholder')"
                    class="w-10rem" :minDate="queryParam.fromDate" :invalid="!!errMsg.toDate"></Calendar>
                <Button @click="onClickSearch" :label="t('body.report.apply_button_1')"
                    :loading="loading.export"></Button>
            </div>
        </div>
        <Divider />
        <DataTable v-model:expandedRows="tableSetup.expandedRows" :value="tableSetup.value"
            class="card p-3 overflow-hidden mb-5 border-right-1">
            <Column expander style="width: 1rem" />
            <Column :header="col.header" :field="col.field" :class="col.class" :key="i"
                v-for="(col, i) in tableSetup.columns">
            </Column>
            <template #expansion="slotProps">
                <DataTable :value="slotProps.data.data" stripedRows showGridlines>
                    <Column :field="subCol.field" :header="subCol.header" :class="subCol.class" :key="j"
                        v-for="(subCol, j) in tableSetup.subColumns">
                        <template #body="sp" v-if="subCol.body">
                            {{ subCol.body(sp) }}
                        </template>
                    </Column>
                    <ColumnGroup type="footer">
                        <Row>
                            <Column :colspan="subCol.colspan"
                                :footer="formatNumber(slotProps.data.footer[(subCol.field) as any])"
                                :class="subCol.class" :key="j" v-for="(subCol, j) in tableSetup.subFooter"></Column>
                        </Row>
                    </ColumnGroup>
                </DataTable>
            </template>
            <template #empty>
                <div class="my-5 py-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
            </template>
        </DataTable>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { format } from "date-fns";
import API from "@/api/api-main";
import lodashChain from "lodash";
import { useToast } from "primevue/usetoast";
import { Validator, ValidateOption } from "@/helpers/validate";
import { useI18n } from "vue-i18n";
import ExcelJS from 'exceljs';

const { t } = useI18n();

const tranformData = (data: any) => {
    return lodashChain(data)
        .groupBy("cardId")
        .map((items, cardId) => ({
            cardId: parseInt(cardId),
            cardCode: items[0].cardCode,
            cardName: items[0].cardName,
            footer: {
                value: items.reduce((acc: number, item: any) => {
                    return acc + item.value;
                }, 0),
                valueInvoice: items.reduce((acc: number, item: any) => {
                    return acc + item.valueInvoice;
                }, 0),
                bonus: items.reduce((acc: number, item: any) => {
                    return acc + item.bonus;
                }, 0),
                total: items.reduce((acc: number, item: any) => {
                    return acc + item.total;
                }, 0),
            },
            data: items,
        }))
        .value();
};
const toast = useToast();
const loading = reactive({
    global: false,
    export: false
});

const vldOpt: ValidateOption = {
    fromDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn ngày bắt đầu",
        },
    },
    toDate: {
        validators: {
            required: true,
            nullMessage: "Vui lòng chọn ngày kết thúc",
        },
    },
};
const errMsg = ref({
    fromDate: "",
    toDate: "",
});
const onClickSearch = () => {
    errMsg.value = {
        fromDate: "",
        toDate: "",
    };
    const vresult = Validator(queryParam, vldOpt);
    if (vresult.result === false) {
        errMsg.value = vresult.errors as any;
        return;
    }
    loading.global = true;
    API.get(`report/paynow?${queryParam.toQueryString()}`)
        .then((res) => {
            tableSetup.value = tranformData(res.data);
            tableSetup.expandedRows = tableSetup.value;
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
            console.error(error);
        })
        .finally(() => {
            loading.global = false;
        });
};

const queryParam = reactive({
    fromDate: new Date(new Date().getFullYear(), 0, 1),
    toDate: new Date(),
    CardId: undefined,
    toQueryString: (): string => {
        return [
            queryParam.fromDate
                ? `fromDate=${format(queryParam.fromDate, "MM-dd-yyyy")}`
                : null,
            queryParam.toDate
                ? `toDate=${format(queryParam.toDate, "MM-dd-yyyy")}`
                : null,
            queryParam.CardId ? `CardId=${queryParam.CardId}` : null,
        ]
            .filter((item) => item)
            .join("&");
    },
});

interface ColumnSlots {
    data: any;
    node: any;
    column: any;
    field: string;
    index: number;
    frozenRow: boolean;
    editorInitCallback: (event: Event) => void;
    rowTogglerCallback: (event: Event) => void;
}

const tableSetup = reactive({
    expandedRows: [] as any[],
    value: [] as any[],
    total: 0,
    columns: [
        {
            header: t('body.report.table_header_customer_name_1'),
            field: "cardName",
            class: "",
        },
    ],
    subColumns: [
        {
            header: t('body.report.invoice_number'),
            field: "invocieCode",
        },
        {
            header: t('body.report.document_date'),
            field: "docDate",
            body: ({ data }: ColumnSlots): string =>
                format(data["docDate"], "dd/MM/yyyy"),
        },
        {
            header: "Giá trị theo giá niêm yết ",
            field: "value",
            body: ({ data, field }: ColumnSlots) => formatNumber(data[field]),
            class: "text-right",
        },
        {
            header: "Giá trị hóa đơn",
            field: "valueInvoice",
            body: ({ data, field }: ColumnSlots) => formatNumber(data[field]),
            class: "text-right",
        },
        {
            header: "Mức thưởng (%)",
            field: "bonus",
            body: ({ data, field }: ColumnSlots) => formatNumber(data[field]),
            class: "text-right",
        },
        {
            header: "Thành tiền",
            field: "total",
            body: ({ data, field }: ColumnSlots) => formatNumber(data[field]),
            class: "text-right",
        },
    ],
    subFooter: [
        {
            footer: t('body.report.total_rows_label') || "Tổng",
            colspan: 2,
        },
        {
            name: "Giá trị theo giá niêm yết",
            footer: "0",
            class: "text-right",
            field: "value",
        },
        {
            name: "Giá trị hóa đơn",
            footer: "0",
            class: "text-right",
            field: "valueInvoice",
        },
        {
            colspan: 0,
        },
        {
            name: "Thành tiền",
            footer: "0",
            class: "text-right",
            field: "total",
        },
    ],
});

const initialComponent = () => {
    // code here
};

const formatNumber = (num: number) => {
    if (num === undefined) return "";
    if (num === null) return "";
    return Intl.NumberFormat().format(num);
};

// Hàm xuất Excel với ExcelJS
const exportToExcel = async () => {
    loading.export = true;

    try {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('Báo cáo thanh toán ngay');

        // Tiêu đề báo cáo
        worksheet.mergeCells('A1:F1');
        const titleRow = worksheet.getCell('A1');
        titleRow.value = t('body.report.report_title_immediate_payment_bonus').toUpperCase();
        titleRow.font = { bold: true, size: 16 };
        titleRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // Thông tin thời gian
        worksheet.mergeCells('A2:F2');
        const dateRow = worksheet.getCell('A2');
        dateRow.value = `${t('body.report.time_label_3')}: ${format(queryParam.fromDate, 'dd/MM/yyyy')} - ${format(queryParam.toDate, 'dd/MM/yyyy')}`;
        dateRow.font = { italic: true, size: 11 };
        dateRow.alignment = { horizontal: 'center', vertical: 'middle' };

        // Dòng trống
        worksheet.addRow([]);

        // Header chính
        const headerRow = worksheet.addRow([
            t('body.report.invoice_number'),
            t('body.report.document_date'),
            'Giá trị theo giá niêm yết',
            'Giá trị hóa đơn',
            'Mức thưởng (%)',
            'Thành tiền'
        ]);

        headerRow.eachCell((cell) => {
            cell.fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'FF4472C4' }
            };
            cell.font = {
                bold: true,
                color: { argb: 'FFFFFFFF' },
                size: 11
            };
            cell.alignment = { horizontal: 'center', vertical: 'middle', wrapText: true };
            cell.border = {
                top: { style: 'thin' },
                left: { style: 'thin' },
                bottom: { style: 'thin' },
                right: { style: 'thin' }
            };
        });

        const grandTotal = {
            value: 0,
            valueInvoice: 0,
            total: 0
        };

        tableSetup.value.forEach((customer: any) => {
            const customerRow = worksheet.addRow([
                customer.cardCode,
                customer.cardName,
                '', '', '', ''
            ]);

            worksheet.mergeCells(customerRow.number, 2, customerRow.number, 6);

            customerRow.eachCell((cell) => {
                cell.fill = {
                    type: 'pattern',
                    pattern: 'solid',
                    fgColor: { argb: 'FF70AD47' }
                };
                cell.font = {
                    bold: true,
                    color: { argb: 'FFFFFFFF' },
                    size: 11
                };
                cell.alignment = { horizontal: 'left', vertical: 'middle' };
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' }
                };
            });

            customer.data.forEach((invoice: any) => {
                const dataRow = worksheet.addRow([
                    invoice.invocieCode,
                    format(new Date(invoice.docDate), 'dd/MM/yyyy'),
                    invoice.value,
                    invoice.valueInvoice,
                    invoice.bonus,
                    invoice.total
                ]);

                dataRow.eachCell((cell, colNumber) => {
                    cell.alignment = {
                        horizontal: colNumber >= 3 ? 'right' : 'left',
                        vertical: 'middle'
                    };
                    cell.border = {
                        top: { style: 'thin' },
                        left: { style: 'thin' },
                        bottom: { style: 'thin' },
                        right: { style: 'thin' }
                    };

                    // Format số với 2 chữ số thập phân
                    if (colNumber >= 3) {
                        cell.numFmt = '#,##0.00'; // Hiển thị 2 chữ số thập phân
                    }
                });
            });

            const footerRow = worksheet.addRow([
                '',
                t('body.productManagement.total_label'),
                customer.footer.value,
                customer.footer.valueInvoice,
                '',
                customer.footer.total
            ]);

            footerRow.eachCell((cell, colNumber) => {
                cell.fill = {
                    type: 'pattern',
                    pattern: 'solid',
                    fgColor: { argb: 'FFB4C7E7' }
                };
                cell.font = {
                    bold: true,
                    size: 10
                };
                cell.alignment = {
                    horizontal: colNumber >= 3 ? 'right' : 'left',
                    vertical: 'middle'
                };
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' }
                };

                // Format số với 2 chữ số thập phân (bỏ cột 5 - Mức thưởng)
                if (colNumber >= 3 && colNumber !== 5) {
                    cell.numFmt = '#,##0.00'; // Hiển thị 2 chữ số thập phân
                }
            });

            grandTotal.value += customer.footer.value;
            grandTotal.valueInvoice += customer.footer.valueInvoice;
            grandTotal.total += customer.footer.total;

            worksheet.addRow([]);
        });

        const grandTotalRow = worksheet.addRow([
            '',
            'TỔNG CỘNG',
            grandTotal.value,
            grandTotal.valueInvoice,
            '',
            grandTotal.total
        ]);

        grandTotalRow.eachCell((cell, colNumber) => {
            cell.fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'FFFFC000' }
            };
            cell.font = {
                bold: true,
                color: { argb: 'FFFFFFFF' },
                size: 12
            };
            cell.alignment = {
                horizontal: colNumber >= 3 ? 'right' : 'center',
                vertical: 'middle'
            };
            cell.border = {
                top: { style: 'medium' },
                left: { style: 'medium' },
                bottom: { style: 'medium' },
                right: { style: 'medium' }
            }; 
            if (colNumber >= 3 && colNumber !== 5) {
                cell.numFmt = '#,##0.00';  
            }
        });

        worksheet.columns = [
            { width: 20 },
            { width: 15 },
            { width: 20 },
            { width: 20 },
            { width: 15 },
            { width: 20 }
        ];

        worksheet.views = [
            { state: 'frozen', xSplit: 0, ySplit: 4 }
        ];

        const today = new Date();
        const fileName = `Baocaothanhtoanngay_${format(today, 'ddMMyyyy')}.xlsx`;

        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], {
            type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        });
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = fileName;
        link.click();
        window.URL.revokeObjectURL(url);

        toast.add({
            severity: 'success',
            summary: t('body.systemSetting.success_label'),
            detail: t('body.report.export_success'),
            life: 3000
        });
    } catch (error) {
        console.error('Export error:', error);
        toast.add({
            severity: 'error',
            summary: t('body.report.error_occurred_message'),
            detail: t('body.report.export_failed'),
            life: 3000
        });
    } finally {
        loading.export = false;
    }
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
