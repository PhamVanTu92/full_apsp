<template>
    <div class="confirmation-page">
        <div class="confirmation-header">
            <div>
                <h4 class="confirmation-title">
                    {{ t("client.confirmation_minutes_title") }}
                </h4>
                <div class="confirmation-subtitle">
                    {{ t("client.confirmation_minutes_description") }}
                </div>
            </div>
            <Button
                class="create-button"
                icon="pi pi-plus"
                :label="t('client.create_confirmation_minutes')"
                @click="openCreateDialog"
            />
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
                                :placeholder="t('client.confirmation_minutes_search')"
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
                <Column field="customerName" :header="t('client.customer_name')" class="customer-column">
                    <template #body="{ data }">
                        <div class="customer-name">{{ data.customerName }}</div>
                        <div class="customer-code">{{ data.customerCode }}</div>
                    </template>
                </Column>
                <Column field="createdAt" :header="t('client.created_date')" class="date-column">
                    <template #body="{ data }">
                        {{ data.createdAt }}
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
                                v-if="data.canSend"
                                icon="pi pi-send"
                                text
                                rounded
                                class="send-button"
                                :aria-label="t('client.confirmation_minutes')"
                                :loading="sendingId === data.id"
                                @click="sendMinutes(data)"
                            />
                            <span v-else class="action-spacer"></span>
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

        <Dialog
            v-model:visible="createDialogVisible"
            modal
            header="Tải Lên biên bản mới"
            class="create-minutes-dialog"
            :draggable="false"
            :style="{ width: '960px', maxWidth: 'calc(100vw - 2rem)' }"
            @hide="resetCreateForm"
        >
            <div class="create-minutes-form">
                <button
                    class="upload-dropzone"
                    type="button"
                    :class="{ 'is-dragging': isDraggingFile, 'has-file': selectedFile }"
                    @click="triggerFileInput"
                    @dragenter.prevent="isDraggingFile = true"
                    @dragover.prevent="isDraggingFile = true"
                    @dragleave.prevent="isDraggingFile = false"
                    @drop.prevent="handleFileDrop"
                >
                    <span class="upload-icon">
                        <i class="pi pi-upload"></i>
                    </span>
                    <span class="upload-title">
                        {{ selectedFile ? selectedFile.name : "Click để chọn file hoặc kéo thả" }}
                    </span>
                    <span class="upload-hint">
                        Hỗ trợ PDF, DOC, Image (Max 10MB)
                    </span>
                    <span v-if="fileError" class="upload-error">
                        {{ fileError }}
                    </span>
                </button>

                <input
                    ref="fileInputRef"
                    class="hidden-file-input"
                    type="file"
                    accept=".pdf,.doc,.docx,image/*"
                    @change="handleFileSelect"
                />

                <div class="customer-field">
                    <label for="confirmation-customer">Chọn khách hàng</label>
                    <CustomerSelector
                        id="confirmation-customer"
                        v-model="selectedCustomerCode"
                        :filted="true"
                        :width="'100%'"
                        placeholder="Chọn khách hàng"
                        @item-select="onSelectCustomer"
                        @clear="clearSelectedCustomer"
                    />
                </div>
            </div>

            <template #footer>
                <Button label="Hủy" text severity="secondary" @click="closeCreateDialog" />
                <Button
                    label="Tạo"
                    icon="pi pi-check"
                    :loading="creatingMinutes"
                    :disabled="isCreateFormDisabled"
                    @click="createMinutes(false)"
                />
                <Button
                    label="Tạo và gửi"
                    icon="pi pi-send"
                    :loading="creatingAndSendingMinutes"
                    :disabled="isCreateFormDisabled"
                    @click="createMinutes(true)"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useToast } from "primevue/usetoast";
import {
    createConfirmation,
    getConfirmations,
    sendConfirmation,
} from "@/services/confirmation.service";

const { t } = useI18n();
const router = useRouter();
const toast = useToast();

const keyword = ref("");
const createDialogVisible = ref(false);
const fileInputRef = ref(null);
const selectedFile = ref(null);
const selectedCustomer = ref(null);
const selectedCustomerCode = ref(null);
const isDraggingFile = ref(false);
const fileError = ref("");
const loadingRecords = ref(false);
const creatingMinutes = ref(false);
const creatingAndSendingMinutes = ref(false);
const sendingId = ref(null);
const maxFileSize = 10 * 1024 * 1024;
const allowedFileExtensions = [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png", ".gif", ".webp"];
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
        [item.code, item.fileName, item.customerName, item.customerCode]
            .map(normalize)
            .some((value) => value.includes(searchText))
    );
});

const isCreateFormDisabled = computed(() =>
    !selectedFile.value ||
    !selectedCustomer.value ||
    creatingMinutes.value ||
    creatingAndSendingMinutes.value
);

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
    router.push({ name: "admin-confirmation-minutes-detail", params: { id } });
};

const openCreateDialog = () => {
    createDialogVisible.value = true;
};

const closeCreateDialog = () => {
    createDialogVisible.value = false;
};

const resetCreateForm = () => {
    selectedFile.value = null;
    selectedCustomer.value = null;
    selectedCustomerCode.value = null;
    isDraggingFile.value = false;
    fileError.value = "";
    if (fileInputRef.value) {
        fileInputRef.value.value = "";
    }
};

const triggerFileInput = () => {
    fileInputRef.value?.click();
};

const setSelectedFile = (file) => {
    if (!file) return;

    const fileName = file.name.toLowerCase();
    const isAllowedType = allowedFileExtensions.some((extension) => fileName.endsWith(extension));

    if (!isAllowedType) {
        selectedFile.value = null;
        fileError.value = "File không đúng định dạng hỗ trợ";
        return;
    }

    if (file.size > maxFileSize) {
        selectedFile.value = null;
        fileError.value = "Dung lượng file vượt quá 10MB";
        return;
    }

    fileError.value = "";
    selectedFile.value = file;
};

const handleFileSelect = (event) => {
    setSelectedFile(event.target.files?.[0]);
};

const handleFileDrop = (event) => {
    isDraggingFile.value = false;
    setSelectedFile(event.dataTransfer.files?.[0]);
};

const onSelectCustomer = (customer) => {
    selectedCustomer.value = customer;
};

const clearSelectedCustomer = () => {
    selectedCustomer.value = null;
    selectedCustomerCode.value = null;
};

const getCreatedConfirmationId = (response) => {
    const sources = [
        response?.data,
        response?.data?.data,
        response?.data?.item,
        response?.data?.result,
        response?.data?.items?.[0],
    ];

    for (const source of sources) {
        const id = source?.id || source?.confirmationId;
        if (id) return id;
    }

    return null;
};

const findCreatedRecord = (items, fileName, customer) => {
    const customerCode = customer?.cardCode || customer?.customerCode || customer?.code || "";
    const candidates = items.filter((item) =>
        item.canSend &&
        item.fileName === fileName &&
        (!customerCode || item.customerCode === customerCode)
    );

    return candidates.sort((a, b) => Number(b.id) - Number(a.id))[0];
};

const createMinutes = async (shouldSend = false) => {
    if (!selectedFile.value || !selectedCustomer.value) return;

    const fileName = selectedFile.value.name;
    const customer = selectedCustomer.value;

    try {
        if (shouldSend) {
            creatingAndSendingMinutes.value = true;
        } else {
            creatingMinutes.value = true;
        }

        const response = await createConfirmation({
            file: selectedFile.value,
            customer,
        });

        let refreshedRecords = null;
        let sentCreatedRecord = false;

        if (shouldSend) {
            let createdId = getCreatedConfirmationId(response);

            if (!createdId) {
                const res = await getConfirmations({
                    page: 1,
                    pageSize: 100,
                });
                refreshedRecords = res.items;
                createdId = findCreatedRecord(refreshedRecords, fileName, customer)?.id;
            }

            if (!createdId) {
                notify("warn", "Thông báo", "Đã tạo biên bản, nhưng chưa xác định được mã để gửi tự động");
            } else {
                try {
                    const sendRes = await sendConfirmation(createdId, "", {
                        id: createdId,
                        fileName,
                        customerName: customer?.cardName || customer?.customerName,
                        customerCode: customer?.cardCode || customer?.customerCode || customer?.code,
                        cardId: customer?.id || customer?.cardId || customer?.customerId,
                    });
                    sentCreatedRecord = true;
                    notify("success", "Thành công", "Đã tạo và gửi biên bản cho khách hàng");
                    if (sendRes.notificationError) {
                        notify("warn", "Thông báo", "Đã gửi biên bản, nhưng tạo thông báo cho khách hàng thất bại");
                    }
                } catch (sendError) {
                    notify("error", "Lỗi", getErrorMessage(sendError, "Đã tạo biên bản nhưng gửi cho khách hàng thất bại"));
                }
            }
        } else {
            notify("success", "Thành công", "Đã tạo biên bản xác nhận");
        }

        createDialogVisible.value = false;

        if (refreshedRecords && !sentCreatedRecord) {
            records.value = refreshedRecords;
        } else {
            await fetchRecords();
        }
    } catch (error) {
        notify("error", "Lỗi", getErrorMessage(error, shouldSend ? "Tạo và gửi biên bản thất bại" : "Tạo biên bản thất bại"));
    } finally {
        creatingMinutes.value = false;
        creatingAndSendingMinutes.value = false;
    }
};

const sendMinutes = async (record) => {
    if (!record?.id) return;

    try {
        sendingId.value = record.id;
        const res = await sendConfirmation(record.id, "", record);
        notify("success", "Thành công", "Đã gửi biên bản cho khách hàng");
        if (res.notificationError) {
            notify("warn", "Thông báo", "Đã gửi biên bản, nhưng tạo thông báo cho khách hàng thất bại");
        }
        await fetchRecords();
    } catch (error) {
        notify("error", "Lỗi", getErrorMessage(error, "Gửi biên bản thất bại"));
    } finally {
        sendingId.value = null;
    }
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
    margin-bottom: 2.25rem;
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

.create-button {
    background: #4f46e5;
    border-color: #4f46e5;
    border-radius: 6px;
    box-shadow: 0 6px 14px rgba(79, 70, 229, 0.22);
    flex-shrink: 0;
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

.file-name,
.customer-name {
    color: #0f172a;
    font-weight: 700;
}

.file-name {
    word-break: break-word;
}

.customer-code,
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

.create-minutes-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
}

.upload-dropzone {
    align-items: center;
    background: #f8fafc;
    border: 1px dashed #dbe3ee;
    border-radius: 6px;
    color: #0f172a;
    cursor: pointer;
    display: flex;
    flex-direction: column;
    justify-content: center;
    min-height: 9rem;
    padding: 1.4rem;
    text-align: center;
    transition: background-color 0.15s ease, border-color 0.15s ease;
    width: 100%;
}

.upload-dropzone:hover,
.upload-dropzone.is-dragging {
    background: #f5f7ff;
    border-color: #4f46e5;
}

.upload-dropzone.has-file {
    background: #f8fafc;
    border-color: #a5b4fc;
}

.upload-icon {
    align-items: center;
    background: #eef2ff;
    border-radius: 999px;
    color: #4f46e5;
    display: inline-flex;
    height: 2.8rem;
    justify-content: center;
    margin-bottom: 0.75rem;
    width: 2.8rem;
}

.upload-icon i {
    font-size: 1.35rem;
}

.upload-title {
    color: #0f172a;
    font-size: 0.9rem;
    font-weight: 700;
    line-height: 1.35;
    max-width: 100%;
    overflow-wrap: anywhere;
}

.upload-hint {
    color: #64748b;
    font-size: 0.75rem;
    margin-top: 0.25rem;
}

.upload-error {
    color: #dc2626;
    font-size: 0.75rem;
    margin-top: 0.35rem;
}

.hidden-file-input {
    display: none;
}

.customer-field label {
    color: #334155;
    display: block;
    font-size: 0.8rem;
    margin-bottom: 0.25rem;
}

.customer-field :deep(.p-dropdown) {
    border-color: #dbe3ee;
    border-radius: 6px;
}

.customer-field :deep(.p-inputgroup) {
    width: 100%;
}

.customer-field :deep(.p-autocomplete) {
    width: 100%;
}

.customer-field :deep(.p-autocomplete-input) {
    border-color: #dbe3ee;
    border-radius: 6px 0 0 6px;
}

.customer-field :deep(.p-button) {
    border-color: #dbe3ee;
}

:deep(.create-minutes-dialog .p-dialog-header) {
    border-bottom: 1px solid #edf2f7;
    padding: 1.25rem 1.25rem 1rem;
}

:deep(.create-minutes-dialog .p-dialog-title) {
    color: #111827;
    font-size: 0.98rem;
    font-weight: 800;
    letter-spacing: 0;
}

:deep(.create-minutes-dialog .p-dialog-content) {
    padding: 1.25rem;
}

:deep(.create-minutes-dialog .p-dialog-footer) {
    background: #f8fafc;
    border-top: 1px solid #edf2f7;
    padding: 0.95rem 1.25rem;
}

:deep(.create-minutes-dialog .p-dialog-footer .p-button) {
    color: #475569;
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

    .create-button,
    .filter-button {
        width: 100%;
    }
}
</style>
