<script setup>
import { ref, onMounted, reactive } from 'vue';
import { useRouter } from 'vue-router';
const router = useRouter();
import API from '@/api/api-main';
import { format } from 'date-fns';
import { uniq } from 'lodash';
import _ from 'lodash';
import ExcelJS from 'exceljs';
import { useI18n } from 'vue-i18n'

const { t } = useI18n()

const visibleFilter = ref(false);
const visibleDetail = ref(false);
const loading = ref(false);
const currentDate = new Date();
const dateFilter = reactive({
    startDate: new Date(`${currentDate.getFullYear()}-01-01`),
    endDate: currentDate,
    getQuery: () => {
        const startDateStr = format(dateFilter.startDate, 'yyyy-MM-dd');
        const endDateStr = format(dateFilter.endDate, 'yyyy-MM-dd');
        return `?startDate=${startDateStr}&endDate=${endDateStr}`;
    }
});

const onDateSelected = () => {
    if (dateFilter.startDate && dateFilter.endDate) {
        onClickGetData();
    }
};

const fieldFilters = ['brand', 'industry', 'itemType', 'packaging'];

var taskDoneCount = ref(0);
const fetchReportData = (dateFilter = null) => {
    taskDoneCount.value = 0;
    let url = `report/purchases/top/5/${dateFilter ? dateFilter.getQuery() : ''}`;
    API.get(url)
        .then((res) => {
            const labels = res.data.map((el) => el.itemName);
            const datasets = res.data.map((el) => el.quantity);
            chartData.value = setChartData(labels, datasets);
            taskDoneCount.value++;
        })
        .catch((error) => { });
    API.get(`report/purchases${dateFilter ? dateFilter.getQuery() : ''}`)
        .then((res) => {
            productData.value = sortArray(res.data.lines, 'purchasedQuantity', true);
            taskDoneCount.value++;
            filterOptions;
            fieldFilters.forEach((field) => {
                filterOptions[field] = uniq(sortArray(res.data.lines, 'purchasedQuantity', true).map((el) => el[field]));
            });
        })
        .catch((error) => { });
};

const loadingDetail = ref(false);
const product = ref({});
const fetchDetailProduct = (itemCode) => {
    loadingDetail.value = true;
    API.get(`report/purchases/${itemCode}/${dateFilter ? dateFilter.getQuery() : ''}`)
        .then((res) => {
            product.value.invoices = res.data;
            loadingDetail.value = false;
        })
        .catch((error) => {
            loadingDetail.value = false;
        });
};

const onHideDetailDlg = () => {
    product.value = {};
};

const onClickGetData = () => {
    fetchReportData(dateFilter);
};

const openDetail = (data) => {
    visibleDetail.value = true;
    product.value.product = data;
    fetchDetailProduct(data.itemCode);
};

const chartData = ref({});

const productData = ref([]);
const setChartData = (labels, datasets) => {
    const documentStyle = getComputedStyle(document.documentElement);
    return {
        labels: labels || [],
        datasets: [
            {
                label: t('body.PurchaseRequestList.products_tab'),
                backgroundColor: documentStyle.getPropertyValue('--cyan-500'),
                borderColor: documentStyle.getPropertyValue('--cyan-500'),
                data: datasets
            }
        ]
    };
};

const setChartOptions = () => {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    return {
        indexAxis: 'y',
        maintainAspectRatio: false,
        aspectRatio: 0.8,
        plugins: {
            title: {
                display: true,
                text: t('body.home.most_purchased_products_title'),
                fullSize: true,
                font: {
                    size: 24
                }
            },
            legend: {
                labels: {
                    color: textColor
                }
            }
        },
        scales: {
            x: {
                ticks: {
                    color: textColorSecondary,
                    font: {
                        weight: 500
                    },
                    stepSize: 1,
                    beginAtZero: true
                },
                grid: {
                    display: true,
                    drawBorder: true
                }
            },
            y: {
                ticks: {
                    color: textColorSecondary
                },
                grid: {
                    color: surfaceBorder,
                    drawBorder: false
                }
            }
        }
    };
};

onMounted(() => {
    onClickGetData();
});

const formatCurency = (num) => {
    if (num != null || num != undefined)
        return Intl.NumberFormat('vi-VI', {
            style: 'decimal'
        }).format(num);
    else return 'NaN';
};

const sortArray = (target, field, reverse = false) => {
    if (!target?.length) return [];
    function compareFn(a, b) {
        if (reverse) {
            if (a[field] < b[field]) return 1;
            if (a[field] > b[field]) return -1;
            return 0;
        } else {
            if (a[field] > b[field]) return 1;
            if (a[field] < b[field]) return -1;
            return 0;
        }
    }
    return target.sort(compareFn);
};

const productDataTable = ref();

const exportToCSV = async () => {
    try {
        if (_.isEmpty(productData.value)) {
            proxy.$notify('E', 'Không dữ liệu để xuất', toast);
            return;
        }
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('Hóa đơn');

        const response = await fetch('/src/assets/images/new-logo-ap.png');
        const imageBlob = await response.blob();
        const reader = new FileReader();
        const base64Logo = await new Promise((resolve) => {
            reader.onloadend = () => resolve(reader.result.split(',')[1]);
            reader.readAsDataURL(imageBlob);
        });

        const logoId = workbook.addImage({
            base64: base64Logo,
            extension: 'png'
        });
        worksheet.addImage(logoId, {
            tl: { col: 5, row: 0 },
            ext: { width: 250, height: 100 }
        });

        const titleRow = worksheet.addRow(['Công Ty Cổ Phần AP Saigon Petro']);
        titleRow.font = { name: 'Times New Roman', size: 13, bold: true };
        titleRow.alignment = { horizontal: 'center', vertical: 'middle' };
        titleRow.border = null;
        worksheet.getRow(titleRow.number).height = 50;

        worksheet.mergeCells('A1:D1');
        worksheet.mergeCells('E1:I1');

        const subTitleRow = worksheet.addRow(['Văn phòng: Lầu 1, 6B Tôn Đức Thắng, Bến Nghé, Quận 1, TPHCM.']);
        subTitleRow.font = { name: 'Times New Roman', size: 13, bold: true };
        subTitleRow.alignment = { horizontal: 'center', vertical: 'middle' };
        subTitleRow.border = null;
        worksheet.mergeCells('A2:D2');
        worksheet.mergeCells('E2:I2');

        const dtFax = worksheet.addRow(['Nhà máy: 990 Nguyễn Thị Định, Phường Thạnh Mỹ Lợi, Tp. Thủ Đức']);
        dtFax.font = { name: 'Times New Roman', size: 13 };
        dtFax.alignment = { horizontal: 'center', vertical: 'middle' };
        dtFax.border = null;
        worksheet.mergeCells('A3:D3');
        worksheet.mergeCells('A4:D4');
        worksheet.mergeCells('A5:I5');

        worksheet.mergeCells('A8:I8');
        worksheet.getRow(8).height = 10;

        const baoCaoMuaHang = worksheet.addRow(['BÁO CÁO MUA HÀNG THEO SẢN PHẨM ']);
        baoCaoMuaHang.font = { name: 'Times New Roman', size: 16, bold: true };
        baoCaoMuaHang.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.mergeCells('A9:I9');

        const apDung = worksheet.addRow([`${t('body.promotion.from_date_column')}: ${format(dateFilter.startDate, 'dd/MM/yyyy')} ${t('body.promotion.to_date_column')} ${format(dateFilter.endDate, 'dd/MM/yyyy')}`]);
        apDung.font = { name: 'Times New Roman', size: 13, italic: true };
        apDung.alignment = { horizontal: 'center', vertical: 'middle' };
        worksheet.mergeCells('A10:I10');
        worksheet.mergeCells('A11:I11');
        worksheet.getRow(11).height = 10;
        worksheet.mergeCells('A12:I12');
        worksheet.mergeCells('A13:I13');
        worksheet.getRow(13).height = 10;
        worksheet.getColumn('A').width = 5; 
        worksheet.getColumn('B').width = 25;
        worksheet.getColumn('C').width = 25;
        worksheet.getColumn('D').width = 25;
        worksheet.getColumn('E').width = 30;
        worksheet.getColumn('F').width = 30;
        worksheet.getColumn('G').width = 20;
        worksheet.getColumn('H').width = 30;
        worksheet.getColumn('I').width = 40;
        worksheet.getColumn('J').width = 40;
        worksheet.getColumn('K').width = 40;
        worksheet.getColumn('D').numFmt = '#,##0';
        worksheet.getColumn('I').numFmt = '#,##0';

        const headerRow = worksheet.addRow(['', 'Mã hàng hóa', 'Tên hàng hóa', 'Thương hiệu', 'Ngành hàng', 'Loại hàng hóa', 'Quy cách bao bì', 'Số lượng mua', 'Số lượng được khuyến mại', 'Tổng sản lượng được tích lũy (Lít)', 'Tổng số đơn hàng']);
        headerRow.font = { name: 'Times New Roman', size: 12, bold: true };
        headerRow.alignment = { horizontal: 'center', vertical: 'middle' };
        headerRow.eachCell({ includeEmpty: true }, (cell) => {
            cell.border = {
                top: { style: 'thin' },
                left: { style: 'thin' },
                bottom: { style: 'thin' },
                right: { style: 'thin' }
            };
        });
        _.forEach(productData.value, (item, index) => {
            const row = worksheet.addRow([`${index}`, item.itemCode, item.itemName, item.brand, item.industry, item.itemType, item.packaging, item.purchasedQuantity, item.promotionalQuantity, item.totalAccumulatedVolume, item.totalOrders]);
            row.font = { name: 'Times New Roman', size: 13 };
            row.eachCell({ includeEmpty: true }, (cell) => {
                cell.border = {
                    top: { style: 'thin' },
                    left: { style: 'thin' },
                    bottom: { style: 'thin' },
                    right: { style: 'thin' }
                };
            });
        });
        const now = new Date();
        const fileName = `BC_Mua_hàng_theo_sản_phẩm_${now.getDate()}-${now.getMonth() + 1}-${now.getFullYear()}_${now.getHours()}-${now.getMinutes()}.xlsx`;
        const buffer = await workbook.xlsx.writeBuffer();
        const blob = new Blob([buffer], { type: 'application/octet-stream' });
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    } catch (error) {
        console.error(error);
    } finally {
        loading.value = false;
    }
};
const onRowClick = ({ data }) => {
    openDetail(data);
};

import { FilterMatchMode, FilterOperator } from 'primevue/api';
const filters = ref({
    global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    brand: {
        value: null,
        matchMode: FilterMatchMode.IN
    },
    industry: {
        value: null,
        matchMode: FilterMatchMode.IN
    },
    itemType: {
        value: null,
        matchMode: FilterMatchMode.IN
    },
    packaging: {
        value: null,
        matchMode: FilterMatchMode.IN
    }
});
const filterOptions = reactive({
    brand: [],
    industry: [],
    itemType: [],
    packaging: []
});
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-4">
        <h4 class="font-bold m-0">{{ t('body.home.product_purchase_report_title') }}</h4>
        <Button :label="t('body.home.back_button')" @click="router.back()" icon="pi pi-arrow-left"
            severity="secondary"/>
    </div>
    <div class="flex align-items-center gap-3">
        <span class="font-bold"> {{ t('body.OrderList.time') }}</span>
        <span class="flex gap-3">
            <Calendar v-model="dateFilter.startDate" class="w-3" :placeholder="t('body.promotion.from_date_column')"
                formatDate="dd/mm/yyyy" :maxDate="dateFilter.endDate" @date-select="onDateSelected"></Calendar>
            <Calendar v-model="dateFilter.endDate" class="w-3" :placeholder="t('body.promotion.to_date_column')"
                formatDate="dd/mm/yyyy" :minDate="dateFilter.startDate" @date-select="onDateSelected"></Calendar>
            <Button v-if="0" label="Áp dụng" @click="onClickGetData"/>
        </span>
    </div>
    <hr />
    <div class="card mb-3">
        <Chart type="bar" :data="chartData" :options="setChartOptions()" class="h-30rem card border-noround pt-2" />
        <DataTable v-model:filters="filters" resizableColumns columnResizeMode="expand" ref="productDataTable"
            :value="productData" showGridlines scrollable filterDisplay="menu" scrollHeight="300px"
            :loading="taskDoneCount == 0" selectionMode="single" @row-click="onRowClick"
            :globalFilterFields="['itemCode', 'itemName']">
            <template #header>
                <div class="flex justify-content-between align-items-center">
                    <span class="text-xl">{{ t('body.home.purchased_products_list_title') }}</span>
                    <div class="flex gap-3">
                        <InputText v-model="filters['global'].value" :placeholder="t('body.home.search_placeholder')">
                        </InputText>
                        <Button icon="pi pi-file-export" outlined severity="info"
                            :label="t('body.home.export_excel_button')" @click="exportToCSV"/>
                    </div>
                </div>
            </template>
            <template #empty>
                <div class="my-5 py-5 text-center">{{ t('body.systemSetting.no_data_to_display') }}</div>
            </template>
            <Column header="#" style="width: 3rem">
                <template #body="{ index }">{{ index + 1 }}</template>
            </Column>
            <Column field="itemCode" :header="t('body.home.product_code_column')"></Column>
            <Column field="itemName" :header="t('body.home.product_name_column')"> </Column>
            <Column filterField="brand" :header="t('body.home.brand_column')" field="brand"
                :showFilterMatchModes="false">
                <template #filter="{ filterModel }">
                    <MultiSelect :placeholder="t('body.home.brand_column')" v-model="filterModel.value"
                        :options="filterOptions.brand" class="p-column-filter" style="min-width: 12rem"> </MultiSelect>
                </template>
            </Column>
            <Column :header="t('body.home.category_column')" field="industry" filterField="industry"
                :showFilterMatchModes="false">
                <template #filter="{ filterModel }">
                    <MultiSelect :placeholder="t('body.home.category_column')" v-model="filterModel.value"
                        :options="filterOptions.industry" class="p-column-filter" style="min-width: 12rem">
                    </MultiSelect>
                </template>
            </Column>
            <Column :header="t('body.home.product_type_column')" field="itemType" filterField="itemType"
                :showFilterMatchModes="false">
                <template #filter="{ filterModel }">
                    <MultiSelect :placeholder="t('body.home.product_type_column')" v-model="filterModel.value"
                        :options="filterOptions.itemType" class="p-column-filter" style="min-width: 12rem">
                    </MultiSelect>
                </template>
            </Column>
            <Column :header="t('body.home.packaging_column')" field="packaging" filterField="packaging"
                :showFilterMatchModes="false">
                <template #filter="{ filterModel }">
                    <MultiSelect :placeholder="t('body.home.packaging_column')" v-model="filterModel.value"
                        :options="filterOptions.packaging" class="p-column-filter" style="min-width: 12rem">
                    </MultiSelect>
                </template>
            </Column>
            <Column :header="t('body.home.quantity_column')" field="purchasedQuantity"></Column>
            <Column field="promotionalQuantity" :header="t('body.home.promotional_quantity_column')"></Column>
            <Column :header="t('body.home.total_accumulated_quantity_column')" field="totalAccumulatedVolume"></Column>
            <Column :header="t('body.home.total_orders_column')" field="totalOrders"></Column>
            <Column v-if="0">
                <template #body="{ data }">
                    <span class="pi pi-eye cursor-pointer hover:text-green-700" @click="openDetail(data)"></span>
                </template>
            </Column>
        </DataTable>
    </div>
    <Dialog v-model:visible="visibleFilter" modal :header="t('client.filter')" :style="{ width: '45rem' }">
        <div class="card">
            <div class="flex justify-content-between gap-5">
                <div class="w-6">
                    <div class="flex flex-column gap-2">
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">{{ t('body.OrderList.time') }}</label>
                            <Calendar v-model="dates" selectionMode="range" :manualInput="false"
                                :placeholder="t('body.promotion.from_date_column') - t('body.promotion.to_date_column')" />
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">{{ t('body.home.brand_column') }}</label>
                            <MultiSelect :placeholder="t('body.home.brand_column')"></MultiSelect>
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">{{ t('body.home.category_column') }}</label>
                            <MultiSelect :placeholder="t('body.home.category_column')"></MultiSelect>
                        </div>
                    </div>
                </div>
                <div class="w-6">
                    <div class="flex flex-column gap-2">
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">{{ t('body.home.product_type_column') }}</label>
                            <MultiSelect :placeholder="t('body.home.product_type_column')"></MultiSelect>
                        </div>
                        <div class="flex flex-column gap-2">
                            <label class="font-bold" for="">{{ t('body.home.packaging_column') }}</label>
                            <MultiSelect :placeholder="t('body.home.packaging_column')"></MultiSelect>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button type="button" :label="t('body.home.cancel_button')" severity="secondary"
                    @click="visibleFilter = false"/>
                <Button type="button" :label="t('body.home.confirm_button')" @click="visibleFilter = false"/>
            </div>
        </template>
    </Dialog>
    <Dialog v-model:visible="visibleDetail" modal :header="t('body.home.product_purchase_detail_report_title')"
        :style="{ width: '85rem' }" @hide="onHideDetailDlg">
        <DataTable :value="product.invoices?.lines" showGridlines tableStyle="min-width: 50rem"
            :loading="loadingDetail">
            <template #header>
                <div class="flex gap-3 align-items-center">
                    <div>{{ product.product.itemCode }}</div>
                    <div class="h-2rem border-right-1 border-400"></div>
                    <div>{{ product.product.itemName }}</div>
                </div>
            </template>
            <Column header="#">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="orderCode" :header="t('body.OrderList.orderCode')">
                <template #body="{ data }">
                    <router-link target="_blank" :to="{
                        name: 'order-detail',
                        params: { id: data.orderId }
                    }" class="text-primary font-semibold hover:underline">{{ data.orderCode }}</router-link>
                </template>
            </Column>
            <Column field="orderDate" :header="t('body.home.order_date_column')">
                <template #body="{ data }">
                    {{ format(data.orderDate, 'dd/MM/yyyy') }}
                </template>
            </Column>
            <Column field="quantityPurchased" :header="t('body.home.quantity_column')">
                <template #body="{ data }">
                    <div class="text-right">
                        {{ data.quantityPurchased }}
                    </div>
                </template>
            </Column>
            <Column field="promotionalQuantity" :header="t('body.home.promotional_quantity_column')">
                <template #body="{ data }">
                    <div class="text-right">
                        {{ data.promotionalQuantity }}
                    </div>
                </template>
            </Column>
            <Column field="unitPrice" :header="t('body.home.unit_price_column')">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.unitPrice) }}
                    </div>
                </template>
            </Column>
            <Column field="discount" :header="t('client.discount')">
                <template #body="{ data }">
                    <div class="text-right text-primary">{{ formatCurency(data.discount) }} {{ !data.discountType ||
                        data.discountType === 'P' ? '%' : data.currency }}</div>
                </template>
            </Column>
            <Column field="taxTotal" :header="t('body.home.tax_column')">
                <template #body="{ data }">
                    <div class="text-right text-primary">
                        {{ formatCurency(data.taxTotal) }}
                    </div>
                </template>
            </Column>
            <Column field="totalAmount" :header="t('body.home.payment_method_column')">
                <template #body="{ data }">
                    <div>
                        {{ data.paymentType }}
                    </div>
                </template>
            </Column>
            <Column field="isIncludedInProduction" :header="t('body.home.is_included_in_production_column')">
                <template #body="{ data }">
                    {{ data.isIncludedInProduction ? t('body.home.yes') : t('body.home.no') }}
                </template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :footer="t('body.home.total')" :colspan="3" class="surface-100" />
                    <Column class="surface-100">
                        <template #footer>
                            <div class="text-right">
                                {{ product.invoices?.quantityPurchased }}
                            </div>
                        </template>
                    </Column>
                    <Column class="surface-100">
                        <template #footer>
                            <div class="text-right">
                                {{ product.invoices?.promotionalQuantity }}
                            </div>
                        </template>
                    </Column>
                    <Column class="surface-100">
                        <template #footer>
                            <div class="text-primary text-right"></div>
                        </template>
                    </Column>
                    <Column class="surface-100">
                        <template #footer>
                            <!-- <div class="text-primary text-right">{{ formatCurency(product.invoices?.discount) }}%</div> -->
                        </template>
                    </Column>
                    <Column class="surface-100">
                        <template #footer>
                            <div class="text-primary text-right">
                                {{ formatCurency(product.invoices?.taxTotal) }}
                            </div>
                        </template>
                    </Column>
                    <Column class="surface-100">
                        <template #footer>
                            <div class="text-primary text-right">
                                <!-- {{ formatCurency(product.invoices?.totalAmount) }} -->
                            </div>
                        </template>
                    </Column>
                    <Column class="surface-100" />
                </Row>
            </ColumnGroup>
        </DataTable>
        <template #footer>
            <div class="flex justify-content-end gap-2">
                <Button icon="pi pi-times" type="button" :label="t('body.OrderList.close')" severity="secondary"
                    @click="visibleDetail = false"/>
            </div>
        </template>
    </Dialog>
</template>
