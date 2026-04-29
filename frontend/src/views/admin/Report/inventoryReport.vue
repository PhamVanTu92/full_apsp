<template>
    <div>
        <div class="flex justify-content-between align-content-center">
            <h4 class="font-bold m-0">{{ t('body.report.report_title_inventory') }}</h4>
            <div class="flex gap-2">
                <ButtonGoBack/>
                <Button :label="t('body.report.export_excel_button_1')" outlined icon="pi pi-file-export"
                    severity="info" @click="exportToExcel" :loading="loading.export">
                </Button>
            </div>
        </div>
        <div class="flex gap-3 mt-3">
            <span class="font-bold mt-2">{{ t('body.home.time_label') }}</span>
            <div>
                <Calendar v-model="query.FromDate" class="w-10rem" :placeholder="t('body.report.from_date_placeholder')"
                    :maxDate="new Date()" :invalid="errMsg.FromDate ? true : false" />
                <small>{{ errMsg.FromDate }}</small>
            </div>
            <div>
                <Calendar v-model="query.ToDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')"
                    :minDate="query.FromDate" :maxDate="new Date()" :invalid="errMsg.ToDate ? true : false" />
                <small>{{ errMsg.ToDate }}</small>
            </div>
            <div class="w-30rem">
                <CustomerSelector v-model="query.CardCode" selectionMode="multiple" optionValue="cardCode"
                    :invalid="errMsg.CardCode ? true : false" />
                <small>{{ errMsg.CardCode }}</small>
            </div>
            <div>
                <Button @click="onClickConfirm" :label="t('body.report.apply_button_2')"/>
            </div>
        </div>

        <hr />
        <div class="card p-3">
            <DataTable :value="dataTable" showGridlines tableStyle="min-width: 124rem" selectionMode="single"
                @row-click="OpenDetail" scrollable scrollHeight="620px" rowGroupMode="subheader" groupRowsBy="cardCode"
                :pt="{
                    rowgroupheader: {
                        class: 'surface-100'
                    }
                }">
                <template #groupheader="slotProps">
                    <div class="flex align-items-center gap-2">
                        <span class="font-bold">{{ slotProps.data.cardCode }}</span>
                        <Divider layout="vertical"></Divider>
                        <span class="font-bold">{{ slotProps.data.cardName }}</span>
                    </div>
                </template>
                <Column header="#" style="width: 3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column field="itemCode" :header="t('body.report.table_header_product_code_3')"></Column>
                <Column field="itemName" :header="t('body.report.table_header_product_name_3')"></Column>
                <Column field="packagingSpecifications" :header="t('body.report.table_header_packaging_3')"></Column>
                <Column field="category" :header="t('body.report.table_header_category_4')"></Column>
                <Column field="brand" :header="t('body.report.table_header_brand_4')"></Column>
                <Column field="productType" :header="t('body.report.table_header_product_type_4')"></Column>
                <Column field="beginQty" :header="t('body.report.table_header_beginning_stock_quantity')"
                    class="text-right">
                    <template #body="{ data, field }">
                        {{ Intl.NumberFormat().format(data[field]) }}
                    </template>
                </Column>
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
                <Column field="endQty" :header="t('body.report.table_header_ending_stock_quantity')" class="text-right">
                    <template #body="{ data, field }">
                        {{ Intl.NumberFormat().format(data[field]) }}
                    </template>
                </Column>
                <Column class="hidden"></Column>
                <ColumnGroup type="footer">
                    <Row>
                        <Column :footer="t('body.home.total')" :colspan="7" />
                        <Column :footer="dTfooter.beginQty" class="text-right" />
                        <Column :footer="dTfooter.inQty" class="text-right" />
                        <Column :footer="dTfooter.outQty" class="text-right" />
                        <Column :footer="dTfooter.endQty" class="text-right" />
                    </Row>
                </ColumnGroup>
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
                <Column :header="t('body.report.table_header_order_code')"></Column>
                <Column :header="t('body.home.time_label')">
                    <template #body="{ data }">
                        <span>{{ format(data.docDate, 'dd/MM/yyyy') }}</span>
                    </template>
                </Column>
                <Column :header="t('body.report.table_header_beginning_stock_quantity')" class="text-right"></Column>
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
                <Column :header="t('body.report.table_header_ending_stock_quantity')" class="text-right"></Column>
                <template #empty>
                    <div class="py-5 my-5 text-center text-500 font-italic">{{
                        t('body.report.no_data_to_display_message_1') }}</div>
                </template>
            </DataTable>
        </div>
    </Dialog>
    <Loading v-if="loading.global" />
    <!--  Bộ lọc -->
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue';
import API from '@/api/api-main';
import { format } from 'date-fns';
import { Validator, ValidateOption } from '@/helpers/validate';
import uniq from 'lodash/uniq';
import { useI18n } from 'vue-i18n';
import ExcelJS from 'exceljs'; 
import { useToast } from 'primevue/usetoast';
const toast = useToast();
const { t } = useI18n(); 
const visibleDetail = ref(false);
const currentDate = new Date(); 
const dataTable = ref([]);

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
    CardCode: null as number[] | null
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
        .map(([key, value]) => (value ? `${key}=${isDate(value) ? format(value as Date, 'yyyy-MM-dd') : `,${Array(value).join(',')},`}` : null))
        .filter((item) => item)
        .join('&');
};

const detailTable = ref<any[]>([]);
const dTfooter = reactive({
    beginQty: '',
    inQty: '',
    outQty: '',
    endQty: ''
});

const sumColumn = (array: any[], field: string): string => {
    let result = array.reduce((sum: number, item: any) => {
        return sum + item[field];
    }, 0);
    return Intl.NumberFormat().format(result);
};

const OpenDetail = (event: any) => {
    detailTable.value = event.data.inventoryDetail;
    visibleDetail.value = true;
};

const loading = reactive({
    global: false,
    export: false
});

const fetchData = (query: string) => {
    API.get(`Report/InventoryReport?${query}`)
        .then((res) => {
            dataTable.value = [];
            Object.keys(dTfooter).forEach((key) => {
                dTfooter[key as keyof typeof dTfooter] = '0';
            });
            if (res.data) {
                dataTable.value = groupArray(res.data, 'cardCode');
                Object.keys(dTfooter).forEach((key) => {
                    dTfooter[key as keyof typeof dTfooter] = sumColumn(res.data, key);
                });
            }
        })
        .catch()
        .finally(() => {
            loading.global = false;
        });
};

const isDate = (value: any): boolean => {
    return value instanceof Date;
};

function groupArray<T>(array: T[], groupBy: string): T[] {
    const result: T[] = [];
    const fields = uniq(array.map((item) => item[groupBy as keyof T]));
    fields.forEach((field) => {
        const group = array.filter((item) => item[groupBy as keyof T] === field);
        result.push(...group);
    });
    return result;
}

// Hàm xuất Excel với ExcelJS
const exportToExcel = async () => {
    if (!dataTable.value || dataTable.value.length === 0) {
        // Thong báo chọn dữ liệu để xuất
        toast.add({ severity: 'warn', summary: t('Notification.title'), detail: t('Notification.no_data_to_export_message_1'), life: 3000 });
        return;
    }

    loading.export = true;
    try {
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('Báo cáo tồn kho hàng gửi');
        worksheet.mergeCells('A1:K1');
        const titleRow = worksheet.getCell('A1');
        titleRow.value = t('body.report.report_title_inventory').toUpperCase();
        titleRow.font = { bold: true, size: 16 };
        titleRow.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.mergeCells('A2:K2');
        const dateRow = worksheet.getCell('A2');
        dateRow.value = `${t('body.home.time_label')}: ${format(query.FromDate, 'dd/MM/yyyy')} - ${format(query.ToDate, 'dd/MM/yyyy')}`;
        dateRow.font = { italic: true, size: 11 };
        dateRow.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.addRow([]);
        const headerRow = worksheet.addRow([
            'STT',
            t('body.report.table_header_product_code_3'),
            t('body.report.table_header_product_name_3'),
            t('body.report.table_header_packaging_3'),
            t('body.report.table_header_category_4'),
            t('body.report.table_header_brand_4'),
            t('body.report.table_header_product_type_4'),
            t('body.report.table_header_beginning_stock_quantity'),
            t('body.report.table_header_imported_quantity'),
            t('body.report.table_header_exported_quantity'),
            t('body.report.table_header_ending_stock_quantity')
        ]);

        headerRow.eachCell((cell) => {
            cell.fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'FF4472C4' } // Xanh dương
            };
            cell.font = {
                bold: true,
                color: { argb: 'FFFFFFFF' }, // Chữ trắng
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
        const groupedData: any = {};
        dataTable.value.forEach((item: any) => {
            if (!groupedData[item.cardCode]) {
                groupedData[item.cardCode] = {
                    cardCode: item.cardCode,
                    cardName: item.cardName,
                    items: []
                };
            }
            groupedData[item.cardCode].items.push(item);
        });

        let rowIndex = 1;

        // Thêm dữ liệu theo group
        Object.values(groupedData).forEach((group: any) => {
            // Header nhóm khách hàng - Xanh lá
            const groupHeaderRow = worksheet.addRow([
                '',
                group.cardCode,
                group.cardName,
                '', '', '', '', '', '', '', ''
            ]);

            worksheet.mergeCells(groupHeaderRow.number, 3, groupHeaderRow.number, 11);

            groupHeaderRow.eachCell((cell) => {
                cell.fill = {
                    type: 'pattern',
                    pattern: 'solid',
                    fgColor: { argb: 'FF70AD47' } // Xanh lá
                };
                cell.font = {
                    bold: true,
                    color: { argb: 'FFFFFFFF' }, // Chữ trắng
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

            // Chi tiết sản phẩm
            group.items.forEach((item: any) => {
                const dataRow = worksheet.addRow([
                    rowIndex++,
                    item.itemCode,
                    item.itemName,
                    item.packagingSpecifications,
                    item.category,
                    item.brand,
                    item.productType,
                    item.beginQty,
                    item.inQty,
                    item.outQty,
                    item.endQty
                ]);

                dataRow.eachCell((cell, colNumber) => {
                    cell.alignment = {
                        horizontal: colNumber >= 8 ? 'right' : 'left',
                        vertical: 'middle'
                    };
                    cell.border = {
                        top: { style: 'thin' },
                        left: { style: 'thin' },
                        bottom: { style: 'thin' },
                        right: { style: 'thin' }
                    };

                    // Format số
                    if (colNumber >= 8) {
                        cell.numFmt = '#,##0';
                    }
                });
            });

            // Dòng trống phân cách
            worksheet.addRow([]);
        });

        // Footer - Tổng cộng
        const footerRow = worksheet.addRow([
            '',
            '',
            '',
            '',
            '',
            '',
            t('body.home.total'),
            parseFloat(dTfooter.beginQty.replace(/,/g, '')),
            parseFloat(dTfooter.inQty.replace(/,/g, '')),
            parseFloat(dTfooter.outQty.replace(/,/g, '')),
            parseFloat(dTfooter.endQty.replace(/,/g, ''))
        ]);

        footerRow.eachCell((cell, colNumber) => {
            cell.fill = {
                type: 'pattern',
                pattern: 'solid',
                fgColor: { argb: 'FFD9E1F2' } // Xanh nhạt
            };
            cell.font = {
                bold: true,
                size: 11
            };
            cell.alignment = {
                horizontal: colNumber >= 8 ? 'right' : 'left',
                vertical: 'middle'
            };
            cell.border = {
                top: { style: 'thin' },
                left: { style: 'thin' },
                bottom: { style: 'thin' },
                right: { style: 'thin' }
            };

            // Format số
            if (colNumber >= 8) {
                cell.numFmt = '#,##0';
            }
        });

        // Tùy chỉnh độ rộng cột
        worksheet.columns = [
            { width: 8 },   // STT
            { width: 15 },  // Mã SP
            { width: 35 },  // Tên SP
            { width: 20 },  // Đóng gói
            { width: 20 },  // Danh mục
            { width: 15 },  // Thương hiệu
            { width: 20 },  // Loại SP
            { width: 15 },  // Tồn đầu
            { width: 15 },  // Nhập
            { width: 15 },  // Xuất
            { width: 15 }   // Tồn cuối
        ];

        worksheet.views = [
            { state: 'frozen', xSplit: 0, ySplit: 4 }
        ];

        // Xuất file
        const today = new Date();
        const fileName = `Baocaotonkhohanggui_${format(today, 'ddMMyyyy')}.xlsx`;

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
    } catch (error) {
        console.error('Export error:', error);
    } finally {
        loading.export = false;
    }
};
</script>

<style scoped lang="css">
small {
    color: var(--red-500);
}
</style>
