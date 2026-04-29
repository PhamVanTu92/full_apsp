<template>
    <div>
        <div>
            <div class="flex justify-content-between align-content-center">
                <h4 class="font-bold m-0">{{ t('body.report.system_log_title') }}</h4>
                <div class="flex gap-2">
                    <ButtonGoBack/>
                </div>
            </div>
            <div class="flex justify-content-between gap-3 align-content-center mt-3">
                <div class="stat-card">
                    <div class="stat-icon primary">
                        <i class="pi pi-chart-line"></i>
                    </div>
                    <div class="stat-content">
                        <h3 class="stat-number">1.200</h3>
                        <span class="stat-label">{{ t('body.report.total_activities_card') }}</span>
                    </div>
                </div>

                <div class="stat-card">
                    <div class="stat-icon success">
                        <i class="pi pi-calendar"></i>
                    </div>
                    <div class="stat-content">
                        <h3 class="stat-number">10</h3>
                        <span class="stat-label">{{ t('body.report.today_activities_card') }}</span>
                    </div>
                </div>

                <div class="stat-card">
                    <div class="stat-icon warning">
                        <i class="pi pi-exclamation-triangle"></i>
                    </div>
                    <div class="stat-content">
                        <h3 class="stat-number">9</h3>
                        <span class="stat-label">{{ t('body.report.warnings_card') }}</span>
                    </div>
                </div>

                <div class="stat-card">
                    <div class="stat-icon danger">
                        <i class="pi pi-times-circle"></i>
                    </div>
                    <div class="stat-content">
                        <h3 class="stat-number">0</h3>
                        <span class="stat-label">{{ t('body.report.failures_card') }}</span>
                    </div>
                </div>
            </div>
            <div class="card flex gap-3 mt-3">
                <span class="font-bold mt-2">{{ t('body.report.time_label_5') }}</span>
                <div>
                    <Calendar v-model="query.FromDate" class="w-10rem" :placeholder="t('body.report.from_date_placeholder')" />
                </div>
                <div>
                    <Calendar v-model="query.ToDate" class="w-10rem" :placeholder="t('body.report.to_date_placeholder')" :minDate="query.FromDate" :maxDate="new Date()" />
                </div>
                <div class="w-30rem">
                    <InputText :placeholder="t('body.report.search_placeholder_3')" v-model="keySearch" class="w-full"></InputText>
                </div>
                <div class="flex gap-1">
                    <Button @click="onClickConfirm" :label="t('body.report.apply_button_3')"/>
                    <Button
                        v-if="keySearch"
                        @click="
                            () => {
                                keySearch = '';
                                page.value = 0;
                                loading.global = true;
                                fetchData('');
                            }
                        "
                        severity="danger"
                        outlined
                        label="Xóa lọc"
                        icon="pi pi-times"
                    />
                </div>
            </div>

            <hr />
            <ScrollPanel style="width: 100%; height: 600px" class="card p-2 shadow-3">
                <DataView :value="dataTable" layout="list" paginator :rows="rows" lazy :totalRecords="totalRecords" :page="page" dataKey="id" class="activity-log-view" @page="onPageChange">
                    <template #list="slotProps">
                        <div class="activity-timeline">
                            <div v-for="(item, index) in slotProps.items" :key="index" class="activity-item">
                                <div class="timeline-marker">
                                    <div class="activity-icon">
                                        <i class="pi pi-sign-in text-white"></i>
                                    </div>
                                    <div v-if="index !== 4" class="timeline-line"></div>
                                </div>
                                <div class="activity-content">
                                    <div class="card">
                                        <div class="activity-header">
                                            <h6 class="activity-title">{{ getStatusLabel(item) }}</h6>
                                            <span class="activity-time">{{ item?.timestamp ? new Date(item.timestamp).toLocaleString('vi-VN') : '' }}</span>
                                        </div>
                                        <p class="activity-description">{{ item.message }}</p>
                                        <div class="activity-tags">
                                            <div class="tag-item bg-purple-500 text-white">
                                                <i class="pi pi-user mr-1"></i>
                                                <span>{{ t('body.report.executor_label') }} {{ item.actorUserName || '--' }}</span>
                                            </div>
                                            <div class="tag-item">
                                                <i class="pi pi-globe mr-1"></i>
                                                <span>{{ t('body.report.table_header_ip_address') }}: {{ item.ipAddress }}</span>
                                            </div>
                                            <div class="tag-item">
                                                <i class="pi pi-desktop mr-1"></i>
                                                <span>{{ t('body.report.table_header_device') }}: {{ item.device }}</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </template>
                </DataView>
            </ScrollPanel>
        </div>

        <!-- Chi tiết từng báo cáo  -->
        <Dialog v-model:visible="visibleDetail" :header="t('body.report.report_detail_title')" modal>
            <div class="card p-3">
                <DataTable :value="dataTable" showGridlines tableStyle="min-width: 50rem">
                    <Column header="#" style="width: 3rem">
                        <template #body="{ index }">{{ index + 1 }}</template>
                    </Column>
                    <Column :header="t('body.report.order_or_request_code_column')"></Column>
                    <Column :header="t('body.home.time_label')">
                        <template #body="{ data }">
                            <span>{{ format(data.docDate, 'dd/MM/yyyy') }}</span>
                        </template>
                    </Column>
                    <Column :header="t('body.report.beginning_inventory_quantity_column')" class="text-right"></Column>
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
                        <div class="py-5 my-5 text-center text-500 font-italic">{{ t('body.report.no_data_to_display_message_1') }}</div>
                    </template>
                </DataTable>
            </div>
        </Dialog>
        <Loading v-if="loading.global" />
    </div>
    <!--  Bộ lọc -->
</template>

<script setup lang="ts">
import { reactive, ref, watchEffect, computed, onMounted } from 'vue';
import API from '@/api/api-main';
import { format, formatDate } from 'date-fns';
import { Validator, ValidateOption } from '@/helpers/validate';
import uniq from 'lodash/uniq';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

interface ActivityLogItem {
    id?: string | number;
    timestamp?: string | Date;
    objType: number;
    objId: number;
    level: number;
    action: string;
    message: string;
    actorId: number;
    actorName: string;
    logger: null;
    exception: null;
    ipAddress: string;
    url: string;
    device: string;
    httpMethod: string;
    requestBody: null;
    responseBody: null;
}

const page = ref(0);
const rows = ref(10);
const totalRecords = ref(0);
const visibleDetail = ref(false);
const currentDate = new Date();
const keySearch = ref('');
onMounted(() => {
    fetchData('');
});
const dataTable = ref<ActivityLogItem[]>([]);

const query = reactive({
    FromDate: new Date(),
    ToDate: currentDate,
    CardCode: null as number[] | null
});

const onClickConfirm = () => {
    page.value = 0; // Reset về trang đầu khi filter
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

const loading = reactive({
    global: false
});
const fetchData = (query: string) => {
    let url = `ActivityLog?Page=${page.value + 1}&PageSize=${rows.value}&OrderBy=id desc&${query ? query : ''}`;
    if (keySearch.value) {
        url += `&search=${keySearch.value}`;
    }
    API.get(url)
        .then((res) => {
            dataTable.value = [];

            if (res.data) {
                dataTable.value = res.data.items;
                totalRecords.value = res.data.total;
            }
        })
        .catch()
        .finally(() => {
            loading.global = false;
        });
};
const onPageChange = (e: any) => {
    page.value = e.page;
    rows.value = e.rows;
    loading.global = true;
    fetchData(toQueryString());
};
const isDate = (value: any): boolean => {
    return value instanceof Date;
};
const getStatusLabel = (data: any) => {
    let label = data.action;
    switch (label) {
         case 'Login':
            return t('body.report.login_success_message');
        case 'ChangeStatus':
            return t('body.report.update_status_button') || 'Cập nhật trạng thái';
        case 'Update':
            return t('body.report.update_button') || 'Cập nhật';
        case 'Delete':
            return t('body.OrderList.delete');
        case 'Import':
            return t('body.report.import_file_button') || 'Nhập File';
        case 'Export':
            return t('body.report.export_file_button') || 'Xuất File';
        case 'Create':
            return t('body.report.create_new_button') || 'Tạo mới';
        default:
            return t('body.report.undefined_label') || 'Không xác định';
    }
};
</script>
<style scoped lang="css">
small {
    color: var(--red-500);
}

/* Simple Stats Cards */
.stat-card {
    background: var(--surface-card);
    border: 1px solid var(--surface-border);
    border-radius: 8px;
    padding: 1.5rem;
    display: flex;
    align-items: center;
    gap: 1rem;
    flex: 1;
    transition: all 0.2s ease;
}

.stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.2rem;
    color: white;
    flex-shrink: 0;
}

.stat-icon.primary {
    background-color: var(--primary-color);
}

.stat-icon.success {
    background-color: var(--green-500);
}

.stat-icon.warning {
    background-color: var(--orange-500);
}

.stat-icon.danger {
    background-color: var(--red-500);
}

.stat-content {
    flex: 1;
}

.stat-number {
    font-size: 1.8rem;
    font-weight: 700;
    margin: 0 0 0.25rem 0;
    color: var(--text-color);
    line-height: 1;
}

.stat-label {
    font-size: 0.875rem;
    color: var(--text-color-secondary);
    font-weight: 500;
    margin: 0;
}

/* Responsive */
@media (max-width: 768px) {
    .flex.justify-content-between {
        flex-direction: column;
        gap: 1rem;
    }

    .stat-card {
        padding: 1rem;
    }

    .stat-icon {
        width: 40px;
        height: 40px;
        font-size: 1rem;
    }

    .stat-number {
        font-size: 1.5rem;
    }

    .stat-label {
        font-size: 0.8rem;
    }
}

/* Activity Timeline Styles */
.activity-log-view {
    background: transparent;
}

.activity-timeline {
    padding: 2rem;
    position: relative;
}

.activity-item {
    display: flex;
    margin-bottom: 2rem;
    position: relative;
    opacity: 0;
    animation: fadeInUp 0.6s ease forwards;
}

.activity-item:nth-child(1) {
    animation-delay: 0.1s;
}
.activity-item:nth-child(2) {
    animation-delay: 0.2s;
}
.activity-item:nth-child(3) {
    animation-delay: 0.3s;
}
.activity-item:nth-child(4) {
    animation-delay: 0.4s;
}
.activity-item:nth-child(5) {
    animation-delay: 0.5s;
}

.timeline-marker {
    display: flex;
    flex-direction: column;
    align-items: center;
    margin-right: 1.5rem;
    position: relative;
}

.activity-icon {
    width: 3rem;
    height: 3rem;
    border-radius: 50%;
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--green-700) 100%);
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 4px 15px rgba(13, 115, 59, 0.4);
    position: relative;
    z-index: 2;
    transition: all 0.3s ease;
}

.activity-icon:hover {
    transform: scale(1.1);
    box-shadow: 0 6px 20px rgba(13, 115, 59, 0.6);
}

.activity-icon i {
    font-size: 1.1rem;
    color: white;
}

.timeline-line {
    width: 2px;
    height: 4rem;
    background: linear-gradient(180deg, var(--primary-color) 0%, var(--green-100) 100%);
    margin-top: 0.5rem;
    opacity: 0.6;
}

.activity-content {
    flex: 1;
    margin-top: -0.5rem;
}

.activity-card {
    background: var(--surface-card);
    border-radius: var(--border-radius);
    padding: 1.5rem;
    box-shadow: 0 2px 20px rgba(0, 0, 0, 0.08);
    border: 1px solid var(--surface-border);
    transition: all 0.3s ease;
    position: relative;
    overflow: hidden;
}

.activity-card::before {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    height: 3px;
    background: linear-gradient(90deg, var(--primary-color) 0%, var(--green-600) 100%);
}

.activity-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 30px rgba(0, 0, 0, 0.15);
    border-color: var(--primary-color);
}

.activity-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 0.75rem;
}

.activity-title {
    margin: 0;
    font-size: 1.1rem;
    font-weight: 600;
    color: var(--text-color);
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--green-700) 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
}

.activity-time {
    font-size: 0.85rem;
    color: var(--text-color-secondary);
    background: var(--surface-100);
    padding: 0.25rem 0.75rem;
    border-radius: 20px;
    font-weight: 500;
}

.activity-description {
    color: var(--text-color-secondary);
    margin: 0 0 1rem 0;
    line-height: 1.6;
    font-size: 0.95rem;
}

.user-highlight {
    background: linear-gradient(135deg, var(--primary-color) 0%, var(--green-700) 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    font-weight: 600;
}

.activity-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
}

.tag-item {
    display: flex;
    align-items: center;
    padding: 0.4rem 0.8rem;
    background: var(--surface-100);
    border: 1px solid var(--surface-200);
    border-radius: 20px;
    font-size: 0.8rem;
    color: var(--text-color-secondary);
    transition: all 0.3s ease;
    font-weight: 500;
}

.tag-item:hover {
    background: var(--surface-200);
    transform: translateY(-1px);
}

.tag-item.success:hover {
    background: linear-gradient(135deg, var(--green-600) 0%, var(--green-700) 100%);
}

.tag-item i {
    font-size: 0.75rem;
}

/* Animations */
@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(30px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* Responsive Design */
@media (max-width: 768px) {
    .activity-timeline {
        padding: 1rem;
    }

    .timeline-marker {
        margin-right: 1rem;
    }

    .activity-icon {
        width: 2.5rem;
        height: 2.5rem;
    }

    .activity-card {
        padding: 1rem;
    }

    .activity-header {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }

    .activity-tags {
        gap: 0.3rem;
    }

    .tag-item {
        font-size: 0.75rem;
        padding: 0.3rem 0.6rem;
    }
}
</style>
