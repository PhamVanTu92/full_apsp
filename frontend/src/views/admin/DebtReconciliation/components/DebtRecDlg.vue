<template>
    <Dialog
        v-model:visible="visible"
        modal
        @hide="onHide"
        class="w-5"
        :header="(modelValue.id ? 'Chi tiết' : 'Tạo mới') + ' biên bản đối chiếu công nợ'"
    >
        <div class="grid" v-if="!editMode">
            <template v-if="modelValue.id">
                <div class="col-4 pb-0">
                    <span class="font-semibold mr-2">Ngày gửi:</span>
                    <span>{{
                        modelValue.createdAt
                            ? format(modelValue.createdAt, "dd/MM/yyyy")
                            : ""
                    }}</span>
                    <!-- {{ modelValue.createdAt }} -->
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
                        :disabled="['A', 'P', 'C', 'R'].includes(modelValue.status)"
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
                        :disabled="['A', 'P', 'C', 'R'].includes(modelValue.status)"
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
                    :readonly="['A', 'P', 'C', 'R'].includes(modelValue.status)"
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
                    :invalid="errorMessage?.attachments ? true : false"
                    class="mb-2"
                    :readonly="true"
                ></DocumentAttach>
                <small class="text-red-500"> {{ errorMessage?.attachments }}</small>
            </div>
            <div v-if="modelValue.rejectReason" class="col-12 pb-0">
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
                        :disabled="['A', 'P', 'C', 'R'].includes(modelValue.status)"
                        v-model="modelValue.note"
                        rows="3"
                        class="w-full"
                    ></Textarea>
                </div>
            </div>
        </div>
        <div class="grid" v-else>
            <template v-if="modelEditValue.id">
                <div class="col-4 pb-0">
                    <span class="font-semibold mr-2">Ngày gửi:</span>
                    <span>{{
                        modelValue.createdAt
                            ? format(modelValue.createdAt, "dd/MM/yyyy")
                            : ""
                    }}</span>
                    <!-- {{ modelValue.createdAt }} -->
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
                        v-model="modelEditValue.name"
                        placeholder="Nhập tên biên bản"
                        :disabled="getActionDisable(modelEditValue.status)"
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
                    <!-- {{ modelEditValue.customerId }} -->
                    <CustomerSelection
                        v-model="modelEditValue.customerId"
                        placeholder="Tìm khách hàng"
                        :disabled="['A', 'P', 'C', 'R'].includes(modelEditValue.status)"
                        :invalid="errorMessage?.customerId ? true : false"
                    ></CustomerSelection>
                    <small class="text-red-500"> {{ errorMessage?.customerId }}</small>
                </div>
            </div>
            <div class="col-12 py-0">
                <DocumentAttach
                    v-model:data="modelEditValue.attachments"
                    header="Tài liệu đính kèm"
                    showGridlines
                    multiple
                    deleteConfirm
                    @delete="onDeleteFileUpdate"
                    fileName="fileName"
                    :invalid="errorMessage?.attachments ? true : false"
                    class="mb-2"
                    :readonly="getActionDisable(modelEditValue.status)"
                ></DocumentAttach>
                <small class="text-red-500"> {{ errorMessage?.attachments }}</small>
            </div>
            <div v-if="modelEditValue.reason" class="col-12 pb-0">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for="">Lý do</label>
                    <div class="p-3 border-left-3 bg-blue-100 border-blue-300">
                        {{ modelEditValue.reason }}
                    </div>
                </div>
            </div>
            <div class="col-12">
                <div class="flex flex-column gap-2">
                    <label class="font-semibold" for="">Ghi chú</label>
                    <Textarea
                        :disabled="getActionDisable(modelEditValue.status)"
                        v-model="modelEditValue.note"
                        rows="3"
                        class="w-full"
                    ></Textarea>
                </div>
            </div>
        </div>
        <template #footer v-if="!editMode">
            <Button @click="visible = false" label="Đóng" severity="secondary"/>
            <Button
                v-if="[null, ''].includes(modelValue.status)"
                @click="onClickSentDebtRec"
                icon="pi pi-send"
                label="Gửi biên bản"
                :loading="props.loading"
            />
            <Button
                v-if="['R'].includes(modelValue.status)"
                @click="onClickChangeStatus('C')"
                icon="pi pi-times"
                label="Hủy biên bản"
                severity="danger"
                :loading="props.loading"
            />
            <Button
                v-if="['R'].includes(modelValue.status)"
                @click="onClickUpdate"
                icon="pi pi-pencil"
                label="Chỉnh sửa"
            />
        </template>
        <template #footer v-else>
            <Button
                @click="editMode = false"
                icon="pi pi-times"
                label="Hủy"
                severity="secondary"
            />
            <Button
                :loading="props.loading"
                @click="onClickResent"
                icon="pi pi-send"
                label="Gửi lại"
            />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch, nextTick } from "vue";
import { searchCustomer, getDebtRecDetail, getActionDisable, getStatus } from "../script";
import { DebtRec, IDebtRec } from "../entities/DebtRec";
import { Customer } from "../entities/Customer";
import { useRoute, useRouter } from "vue-router";
import { format } from "date-fns";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { useConfirm } from "primevue/useconfirm";
import { cloneDeep } from "lodash";

var fileDeleteIds: Array<number> = [];
const onDeleteFileUpdate = (row: any) => {
    if (row.data.id) {
        fileDeleteIds.push(row.data.id);
    }
    modelEditValue.value.attachments?.splice(row.index, 1);

};

const onClickResent = () => {
    const vresult = modelEditValue.value.validate();
    errorMessage.value = vresult.errors as IDebtRec;
    const data = {
        isValid: vresult.result,
        model: modelEditValue.value,
        deleteFiles: fileDeleteIds,
    };
    emits("resent", data);
};

const onClickUpdate = () => {
    editMode.value = true;
    modelEditValue.value = cloneDeep(modelValue.value);
};

const modelEditValue = ref<IDebtRec>(new DebtRec());
const editMode = ref(false);

const toast = useToast();

const emits = defineEmits(["save", "changeStt", "resent"]);
const confirm = useConfirm();
const onClickChangeStatus = (stt: string) => {
    confirm.require({
        message: "Bạn có muốn hủy biên bản này?",
        header: "Xác nhận hủy",
        rejectClass: "p-button-secondary",
        acceptClass: "p-button-danger",
        acceptIcon: "pi pi-check",
        rejectIcon: "pi pi-times",
        rejectLabel: "Hủy",
        acceptLabel: "Xác nhận",
        accept: () => {
            API.update(`DebtReconciliation/${modelValue.value.id}/change-status/${stt}`)
                .then((res) => {
                    toast.add({
                        severity: "success",
                        summary: "Thành công",
                        detail: "Cập nhật thành công",
                        life: 3000,
                    });
                    visible.value = false;
                    emits("changeStt");
                })
                .catch((error) => {
                    toast.add({
                        severity: "error",
                        summary: "Lỗi",
                        detail: "Có lỗi xảy ra, vui lòng thử lại sau.",
                        life: 3000,
                    });
                });
        },
        reject: () => {
            toast.add({
                severity: "error",
                summary: "Rejected",
                detail: "You have rejected",
                life: 3000,
            });
        },
    });
};

const onDeleteFile = (row: any): void => {

    // delete file in server here
    modelValue.value.attachments?.splice(row.index, 1);
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
        const res = await getDebtRecDetail(id);
        modelValue.value = new DebtRec(res.data);
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
    editMode.value = false;
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
</script>

<style scoped></style>
