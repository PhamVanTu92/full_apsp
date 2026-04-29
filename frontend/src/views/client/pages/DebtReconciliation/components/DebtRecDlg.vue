<template>
    <Dialog
        v-model:visible="visible"
        modal
        @hide="onHide"
        class="w-5"
        :header="(modelValue.id ? 'Chi tiết' : 'Tạo mới') + ' biên bản đối chiếu công nợ'"
    >
        <div class="grid">
            <template v-if="modelValue.id">
                <div class="col-4 pb-0">
                    <span class="font-semibold mr-2">Ngày gửi:</span>
                    <span>{{
                        modelValue.sendingDate
                            ? format(modelValue.sendingDate || "", "dd/MM/yyyy")
                            : ""
                    }}</span>
                </div>
                <div class="col-4 pb-0">
                    <span class="font-semibold mr-2">Người gửi:</span>
                    <span>{{ modelValue.creatorName }}</span>
                </div>
                <div class="col-4 pb-0">
                    <span class="font-semibold mr-2">Trạng thái:</span>
                    <Tag
                        :value="getStatus(modelValue.status || 'P').label"
                        :severity="getStatus(modelValue.status || 'P').severity"
                    ></Tag>
                </div>
            </template>
            <div class="col-6">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for=""
                        >Tên biên bản <sup class="text-red-500">*</sup></label
                    >
                    <InputText
                        id="inputName"
                        v-model="modelValue.name"
                        placeholder="Nhập tên biên bản"
                        :disabled="true"
                        :invalid="errorMessage?.name ? true : false"
                    ></InputText>
                    <small class="text-red-500"> {{ errorMessage?.name }}</small>
                </div>
            </div>
            <div class="col-6">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for=""
                        >Khách hàng <sup class="text-red-500">*</sup></label
                    >
                    <!-- {{ modelValue.customerId }} -->
                    <CustomerSelection
                        v-model="modelValue.customerId"
                        placeholder="Tìm khách hàng"
                        :disabled="true"
                        :invalid="errorMessage?.customerId ? true : false"
                    ></CustomerSelection>
                    <small class="text-red-500"> {{ errorMessage?.customerId }}</small>
                </div>
            </div>
            <div class="col-12 py-0">
                <DocumentAttach
                    v-model:data="modelValue.attachments"
                    header="Tài liệu đính kèm"
                    showGridlines
                    multiple
                    deleteConfirm
                    @delete="onDeleteFile"
                    fileName="fileName"
                    :invalid="errorMessage?.attachments ? true : false"
                    class="mb-2"
                    :readonly="true"
                ></DocumentAttach>
                <small class="text-red-500"> {{ errorMessage?.attachments }}</small>
            </div>
            <div v-if="modelValue.status == 'A'" class="col-12 py-0">
                <DocumentAttach
                    v-model:data="modelValue.bpAttachments"
                    header="Tài liệu khách hàng đính kèm"
                    showGridlines
                    multiple
                    deleteConfirm
                    @delete="onDeleteFile"
                    fileName="fileName"
                    class="mb-2"
                    :readonly="true"
                ></DocumentAttach>
                <small class="text-red-500"> {{ errorMessage?.attachments }}</small>
            </div>
            <div v-if="modelValue.status == 'R'" class="col-12 pb-0">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for="">Lý do</label>
                    <div class="p-3 border-left-3 bg-blue-100 border-blue-300">
                        {{ modelValue.rejectReason }}
                    </div>
                </div>
            </div>
            <div class="col-12">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for="">Ghi chú</label>
                    <Textarea
                        :disabled="1"
                        v-model="modelValue.note"
                        rows="3"
                        class="w-full"
                    ></Textarea>
                </div>
            </div>
        </div>
        <template #footer>
            <Button @click="visible = false" label="Đóng" severity="secondary"/>
            <Button
                v-if="['P'].includes(modelValue.status)"
                @click="onClickReject"
                label="Từ chối"
                :loading="props.loading"
                severity="danger"
            />
            <Button
                v-if="['P'].includes(modelValue.status)"
                @click="onClickConfirm"
                label="Xác nhận"
                :loading="props.loading"
            />
        </template>
    </Dialog>
    <Dialog
        v-model:visible="visibleConfirm"
        header="Lý do từ chối"
        modal
        :closable="false"
        class="w-3"
        @hide="cancelReject"
    >
        <Textarea
            id="reason"
            v-model="reason"
            placeholder="Nhập lý do từ chối"
            rows="3"
            class="w-full"
            :invalid="errorReason ? true : false"
        ></Textarea>
        <small class="text-red-500">{{ errorReason }}</small>
        <template #footer>
            <Button
                label="Hủy"
                icon="pi pi-times"
                @click="cancelReject"
                severity="secondary"
            />
            <Button
                label="Xác nhận"
                icon="pi pi-check"
                @click="confirmReject"
                severity="danger"
            />
        </template>
    </Dialog>
    <Dialog
        v-model:visible="visibleConfirmAccecpt"
        header="Xác nhận biên bản đối chiếu công nợ"
        class="w-5"
        modal
    >
        <DocumentAttach
            showGridlines
            multiple
            v-model:data="documentFiles"
            header="Tài liệu đính kèm"
            @delete="onDeleteFileAttach"
        ></DocumentAttach>
        <template #footer>
            <Button
                @click="visibleConfirmAccecpt = false"
                icon="pi pi-times-circle"
                label="Đóng"
                severity="secondary"
            />
            <Button
                @click="onClickAccecpt"
                icon="pi pi-check"
                label="Xác nhận"
                :loading="props.loading"
            />
        </template>
    </Dialog>
    <ConfirmDialog></ConfirmDialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch, nextTick } from "vue";
import { getStatus } from "../script";
import { DebtRec, IDebtRec } from "../entities/DebtRec";
import { Customer } from "../entities/Customer";
import { useRoute, useRouter } from "vue-router";
import { format } from "date-fns";
import API from "@/api/api-main";
import { AxiosResponse } from "axios";
import { useToast } from "primevue/usetoast";

const emits = defineEmits(["save"]);
const toast = useToast();

const changeStt2A = (id: number | any) => {
    API.update(`DebtReconciliation/${id}/change-status/A`)
        .then((res: AxiosResponse<IDebtRec>) => {
            documentFiles.value = [];
            visibleConfirmAccecpt.value = false;
            visible.value = false;
            emits("save");
            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Đã xác nhận biên bản đối chiếu công nợ",
                life: 3000,
            });
        })
        .catch((error) => {
            console.error(error);
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Có lỗi xảy ra",
                life: 3000,
            });
        });
};

const onClickAccecpt = () => {
    if (documentFiles.value.length) {
        const formData = new FormData();
        documentFiles.value.forEach((row: any) => {
            formData.append("files", row.file);
        });
        API.add(`DebtReconciliation/${modelValue.value.id}/bp-attachments`, formData)
            .then(() => {
                changeStt2A(modelValue.value.id);
            })
            .catch((error) => {
                console.error(error);
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Có lỗi xảy ra",
                    life: 3000,
                });
            });
    } else {
        changeStt2A(modelValue.value.id);
    }
};

const onDeleteFileAttach = (e) => {

    documentFiles.value.splice(e.index, 1);
};

const documentFiles = ref([]);
const visibleConfirmAccecpt = ref(false);
const onClickConfirm = () => {
    visibleConfirmAccecpt.value = true;
};

const reason = ref("");
const errorReason = ref("");
const confirmReject = () => {
    if (!reason.value?.trim().length) {
        errorReason.value = "Vui lòng nhập lý do từ chối";
        return;
    } else {
        API.update(
            `DebtReconciliation/${
                modelValue.value.id
            }/change-status/R?rejectReason=${reason.value?.trim()}`
        )
            .then((res) => {
                reason.value = "";
                visibleConfirm.value = false;
                visible.value = false;
                emits("save");
                toast.add({
                    severity: "success",
                    summary: "Thành công",
                    detail: "Đã gửi yêu cầu thành công",
                    life: 3000,
                });
            })
            .catch((error) => {
                toast.add({
                    severity: "error",
                    summary: "Lỗi",
                    detail: "Đã xảy ra lỗi",
                    life: 3000,
                });
            });
    }
};
const cancelReject = () => {
    reason.value = "";
    errorReason.value = "";
    visibleConfirm.value = false;
};

const visibleConfirm = ref(false);
const onClickReject = () => {
    visibleConfirm.value = true;
};

const onDeleteFile = (row: any): void => {

    // delete file in server here
    if (row.data.id) {
    } else {
        modelValue.value.attachments?.splice(row.index, 1);
    }
};

const errorMessage = ref<IDebtRec>();
const onClickSentDebtRec = async (): Promise<void> => {
    const vresult = modelValue.value.validate();
    errorMessage.value = vresult.errors as IDebtRec;
    const data = {
        isValid: vresult.result,
        model: modelValue.value.toJSON(),
    };
    emits("save", data);
};

//------------------------------------------
const inputName = ref<HTMLElement>();

const props = defineProps<{
    id: string | number | null;
    loading?: boolean;
}>();

const visible = defineModel("visible", {
    type: Boolean,
    default: false,
});

const modelValue = ref<IDebtRec>(new DebtRec());

const fetchDetail = async (id: number | any) => {
    try {
        modelValue.value = await getDebtRecDetail(id);
    } catch (error) {
        console.error(error);
        alert("Lỗi tải chi tiết biên bản đối chiếu công nợ");
        router.replace({ name: route.name });
        visible.value = false;
    }
};

const router = useRouter();
const route = useRoute();

const onHide = () => {
    visible.value = false;
    errorMessage.value = {} as IDebtRec;
    router.replace({ name: route.name, query: {} });
};

watch(
    () => visible.value,
    (value) => {
        if (value) {
            if (props.id) {
                fetchDetail(props.id);
            } else {
                modelValue.value = new DebtRec();
            }
        }
    }
);

onMounted(function () {});

const getDebtRecDetail = async (id: number): Promise<IDebtRec> => {
    const res = (await API.get(`DebtReconciliation/${id}`)) as AxiosResponse<IDebtRec>;
    return res.data;
};
</script>

<style scoped></style>
