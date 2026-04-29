<template>
    <div class="confirmation-page">
        <div class="confirmation-header">
            <div>
                <h4 class="confirmation-title">
                    {{ t("client.confirmation_minutes_need_title") }}
                </h4>
                <div class="confirmation-subtitle">
                    {{ t("client.confirmation_minutes_need_description") }}
                </div>
            </div>
        </div>

        <div class="card p-3 confirmation-card">
            <DataTable
                :value="filteredRecords"
                :loading="loadingRecords"
                responsiveLayout="scroll"
                class="table-main confirmation-table"
                dataKey="id"
                stripedRows
                showGridlines
                tableStyle="min-width: 50rem"
            >
                <template #header>
                    <div class="confirmation-table-header">
                        <IconField iconPosition="left" class="search-field">
                            <InputIcon class="pi pi-search" />
                            <InputText
                                v-model="keyword"
                                class="w-full"
                                :placeholder="t('client.confirmation_minutes_client_search')"
                            />
                        </IconField>
                        <Button class="filter-button" icon="pi pi-filter" :label="t('client.filter')" outlined />
                    </div>
                </template>

                <Column field="code" :header="t('client.document_code')" class="code-column">
                    <template #body="{ data }">
                        <span class="code-text">{{ data.code }}</span>
                    </template>
                </Column>
                <Column field="fileName" :header="t('client.document_file')" class="file-column">
                    <template #body="{ data }">
                        <div class="file-cell">
                            <span class="file-icon">
                                <i class="pi pi-file" />
                            </span>
                            <span class="file-name">{{ data.fileName }}</span>
                        </div>
                    </template>
                </Column>
                <Column field="sentAt" :header="t('client.sent_date')" class="date-column">
                    <template #body="{ data }">
                        {{ data.sentAt || data.createdAt }}
                    </template>
                </Column>
                <Column field="status" :header="t('client.status')" class="status-column">
                    <template #body="{ data }">
                        <Tag
                            :class="getStatusClass(data.statusSeverity)"
                            :value="data.statusKey ? t(data.statusKey) : data.statusLabel"
                        />
                    </template>
                </Column>
                <Column :header="t('client.actions')" class="actions-column">
                    <template #body="{ data }">
                        <div class="action-buttons">
                            <Button
                                icon="pi pi-eye"
                                text
                                rounded
                                severity="secondary"
                                class="view-button"
                                :aria-label="t('client.detail')"
                                @click="openDetail(data.id)"
                            />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="empty-state">
                        {{ t("client.no_data_to_display") }}
                    </div>
                </template>
            </DataTable>
        </div>
    </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import { getConfirmations } from "@/services/confirmation.service";

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const keyword = ref("");
const loadingRecords = ref(false);
const records = ref([]);

const normalize = (value) =>
    (value || "")
        .toString()
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/đ/g, "d");

const filteredRecords = computed(() => {
    const searchText = normalize(keyword.value).trim();
    if (!searchText) return records.value;

    return records.value.filter((item) =>
        [item.code, item.fileName]
            .map(normalize)
            .some((value) => value.includes(searchText))
    );
});

const notify = (severity, summary, detail) => {
    toast.add({ severity, summary, detail, life: 3000 });
};

const statusClassMap = {
    success: "text-teal-700 bg-teal-200",
    warning: "text-orange-700 bg-white border-1 border-orange-700",
    danger: "text-red-500 border-red-500 bg-white border-1",
    secondary: "text-white bg-gray-500 border-1 border-gray-500",
    info: "text-blue-700 bg-blue-100 border-1 border-blue-200",
};

const getStatusClass = (severity) =>
    `confirmation-status-tag ${statusClassMap[severity] || "text-gray-700 bg-gray-100 border-1 border-gray-200"}`;

const getErrorMessage = (error, fallback = "Đã có lỗi xảy ra") =>
    error?.response?.data?.errors ||
    error?.response?.data?.message ||
    error?.response?.data?.title ||
    error?.message ||
    fallback;

const fetchRecords = async () => {
    try {
        loadingRecords.value = true;
        const res = await getConfirmations({
            page: 1,
            pageSize: 100,
        });
        records.value = res.items;
    } catch (error) {
        notify("error", "Lỗi", getErrorMessage(error, "Không thể tải danh sách biên bản"));
    } finally {
        loadingRecords.value = false;
    }
};

const openDetail = (id) => {
    router.push({ name: "client-confirmation-minutes-detail", params: { id } });
};

onMounted(fetchRecords);
</script>

<style scoped>
.confirmation-page {
    padding: 0.25rem 0 1.5rem;
}

.confirmation-header {
    align-items: flex-start;
    display: flex;
    gap: 1rem;
    justify-content: space-between;
    margin-bottom: 2rem;
}

.confirmation-title {
    color: #0f172a;
    font-size: 1.35rem;
    font-weight: 800;
    letter-spacing: 0;
    line-height: 1.25;
    margin: 0 0 0.35rem;
}

.confirmation-subtitle {
    color: #64748b;
    font-size: 0.95rem;
}

.confirmation-card {
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(15, 23, 42, 0.1);
}

.confirmation-table-header {
    align-items: center;
    display: flex;
    gap: 1rem;
    justify-content: space-between;
}

.search-field {
    max-width: 28rem;
    width: 100%;
}

.search-field :deep(.p-inputtext),
.filter-button {
    border-color: #dbe3ee;
    border-radius: 6px;
    color: #475569;
}

.filter-button {
    flex-shrink: 0;
}

.file-cell {
    align-items: center;
    display: flex;
    gap: 0.85rem;
    min-width: 15rem;
}

.file-icon {
    align-items: center;
    background: #eef2ff;
    border-radius: 8px;
    color: #4f46e5;
    display: inline-flex;
    height: 2rem;
    justify-content: center;
    width: 2rem;
}

.file-name {
    color: #0f172a;
    font-weight: 700;
}

.file-name {
    word-break: break-word;
}

.code-text {
    color: #475569;
    font-size: 0.84rem;
}

.action-buttons {
    align-items: center;
    display: flex;
    gap: 0.35rem;
}

.send-button {
    color: #4f46e5;
}

.view-button {
    color: #94a3b8;
}

.action-spacer {
    display: inline-block;
    width: 2.5rem;
}

.empty-state {
    color: #64748b;
    padding: 3rem 1rem;
    text-align: center;
}

:deep(.confirmation-table .p-datatable-header) {
    background: #ffffff;
    border: 1px solid #e2e8f0;
    border-bottom: 0;
    border-radius: 6px 6px 0 0;
    padding: 1rem;
}

:deep(.confirmation-table .p-datatable-thead > tr > th) {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    color: #64748b;
    font-size: 0.78rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    padding: 0.95rem 1.5rem;
    text-transform: uppercase;
}

:deep(.confirmation-table .p-datatable-tbody > tr > td) {
    border-color: #e2e8f0;
    color: #1e293b;
    padding: 1.15rem 1.5rem;
    vertical-align: middle;
}

:deep(.confirmation-table .p-datatable-tbody > tr:last-child > td) {
    border-bottom: 0;
}

:deep(.confirmation-table .p-datatable-tbody > tr:hover) {
    background: #fafafa;
}

:deep(.confirmation-status-tag.p-tag) {
    border-radius: 4px;
    font-size: 0.75rem;
    font-weight: 600;
    line-height: 1;
}

:deep(.code-column) {
    width: 10rem;
}

:deep(.date-column) {
    width: 12rem;
}

:deep(.status-column) {
    width: 10.5rem;
}

:deep(.actions-column) {
    text-align: center;
    width: 8rem;
}

@media (max-width: 768px) {
    .confirmation-header,
    .confirmation-table-header {
        align-items: stretch;
        flex-direction: column;
    }

    .filter-button {
        width: 100%;
    }
}
</style>
