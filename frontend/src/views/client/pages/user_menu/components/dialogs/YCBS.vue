<template>
    <div>
        <Dialog
            v-model:visible="visible"
            header="Yêu cầu bổ sung"
            style="width: 60rem"
            modal
            @hide="onHide"
        >
            <div class="field">
                <label class="mb-3">
                    Lí do yêu cầu bổ sung
                    <span class="font-bold text-red-500">*</span>
                </label>
                <div class="flex flex-column gap-3">
                    <div>
                        <Checkbox
                            v-model="payload.limitRequire"
                            input-id="vhmcn"
                            binary
                        ></Checkbox>
                        <label class="ml-2" for="vhmcn">Vượt hạn mức công nợ</label>
                    </div>
                    <div>
                        <Checkbox
                            v-model="payload.limitOverDue"
                            input-id="cnqh"
                            binary
                        ></Checkbox>
                        <label class="ml-2" for="cnqh">Công nợ quá hạn</label>
                    </div>
                    <div>
                        <Checkbox
                            v-model="payload.other"
                            input-id="khac"
                            binary
                            @change="
                                () => {
                                    (errMsg['memo'] = ''), (payload.memo = '');
                                }
                            "
                        ></Checkbox>
                        <label class="ml-2" for="khac">Khác</label>
                    </div>
                    <div v-if="payload.other">
                        <Textarea
                            autofocus
                            v-memo="payload.memo"
                            class="w-full"
                            :rows="3"
                            placeholder="Nhập nội dung"
                            :invalid="!!errMsg['memo']"
                        ></Textarea>
                        <small class="text-red-500" v-if="!!errMsg['memo']">{{
                            errMsg["memo"]
                        }}</small>
                    </div>
                </div>
            </div>

            <DataTable :value="payload.attachFile">
                <Column header="#" class="w-3rem">
                    <template #body="{ index }">{{ index + 1 }}</template>
                </Column>
                <Column header="Tên tài liệu">
                    <template #body="{ data }">
                        <InputText
                            :invalid="submited && !data['note']"
                            v-model="data['note']"
                            class="w-full"
                            placeholder="Nhập tên tài liệu"
                        ></InputText>
                    </template>
                </Column>
                <Column header="Ghi chú">
                    <template #body="{ data }">
                        <InputText
                            v-model="data['memo']"
                            class="w-full"
                            placeholder="Nhập ghi chú"
                        ></InputText>
                    </template>
                </Column>
                <Column class="w-3rem">
                    <template #body="{ index }">
                        <Button
                            @click="payload.attachFile.splice(index, 1)"
                            text
                            icon="pi pi-trash"
                            severity="danger"
                            :disabled="payload.attachFile.length < 2"
                        />
                    </template>
                </Column>
                <template #footer>
                    <Button
                        @click="
                            payload.attachFile.push({
                                filename: '',
                                note: '',
                                memo: '',
                                status: 'A',
                            })
                        "
                        icon="pi pi-plus"
                        label="Thêm dòng"
                        outlined
                    />
                </template>
            </DataTable>
            <template #footer>
                <Button
                    @click="visible = false"
                    label="Đóng"
                    severity="secondary"
                />
                <Button @click="onClickSave" label="Gửi" :loading="loading"/>
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useOrderDetailStore } from "../store/orderDetail";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
const odStore = useOrderDetailStore();
const toast = useToast();
const visible = ref(false);
const loading = ref(false);
const submited = ref(false);
const errMsg = ref<{ [key: string]: string }>({});
const onClickSave = () => {
    if (
        !payload.value.limitOverDue &&
        !payload.value.limitRequire &&
        !payload.value.other
    ) {
        toast.add({
            severity: "warn",
            summary: "Cảnh báo",
            detail: "Vui lòng chọn lý do yêu cầu",
            life: 3000,
        });
        return;
    }

    if (payload.value.other && !payload.value.memo) {
        errMsg.value["memo"] = "Vui lòng nhận lý do";
        return;
    }
    submited.value = true;
    if (payload.value.attachFile.some((item) => item.note == "")) {
        return;
    }

    loading.value = true;
    const formData = new FormData();
    payload.value.id = odStore.order?.id || 0;
    formData.append("document", JSON.stringify(payload.value));
    API.update(`PurchaseOrder/${odStore.order?.id}`, formData)
        .then((res) => {
            toast.add({
                severity: "success",
                summary: "Thành công",
                detail: "Đã gửi thành công",
                life: 3000,
            });
            odStore.fetchStore();
            visible.value = false;
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: "Lỗi",
                detail: "Đã có lỗi xảy ra",
                life: 3000,
            });
        })
        .finally(() => {
            loading.value = false;
        });
};

const payload = ref({
    id: 0,
    limitRequire: false,
    limitOverDue: false,
    other: false,
    memo: "",
    attachFile: [
        {
            filename: "",
            note: "",
            memo: "",
            status: "A",
        },
    ],
});

const onHide = () => {
    submited.value = false;
    errMsg.value = {};
    payload.value = {
        id: 0,
        limitRequire: false,
        limitOverDue: false,
        other: false,
        memo: "",
        attachFile: [
            {
                filename: "",
                note: "",
                memo: "",
                status: "A",
            },
        ],
    };
};

const open = () => {
    visible.value = true;
};

defineExpose({ open });

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
label[for] {
    cursor: pointer;
}
</style>
