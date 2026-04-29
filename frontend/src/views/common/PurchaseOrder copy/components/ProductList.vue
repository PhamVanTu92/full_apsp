<template>
    <div>
        <DataTable
            v-model:selection="selection"
            :value="poStore.model.itemDetail"
            resizableColumns
            columnResizeMode="fit"
            showGridlines
            tableStyle="min-width: 50rem"
            scrollable
        >
            <Column selectionMode="multiple" frozen class="z-5 border-right-1"></Column>
            <Column header="#">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column field="itemName" header="Tên hàng hóa" style="min-width: 30rem">
                <template #body="{ data, field }">
                    <div class="w-full gap-2 flex">
                        <div
                            class="flex-grow-1"
                            style="
                                word-wrap: break-word;
                                white-space: pre-wrap;
                                overflow-wrap: break-word;
                            "
                        >
                            {{ data[field] }}
                        </div>
                        <div>
                            <Tag v-if="data._lastDiscount || data._promotionQuanlity"
                                >Khuyến mại</Tag
                            >
                        </div>
                    </div>
                </template>
            </Column>
            <Column header="Số lượng" class="w-10rem">
                <template #body="{ data }">
                    <InputNumber
                        v-model="data.quantity"
                        :min="1"
                        showButtons
                        pt:input:root:class="w-10rem"
                        placeholder="1"
                    />
                </template>
            </Column>
            <Column field="uomName" header="Đơn vị tính"></Column>
            <Column field="price" :header="`Đơn giá (${poStore.model.currency})`">
                <template #body="{ data, field }">
                    <InputNumber
                        v-if="!props.isClient"
                        v-model="data[field]"
                        :min="0"
                        showButtons
                        pt:input:root:class="w-12rem"
                        placeholder="0"
                    />
                    <div v-else class="text-right">{{ fnum(data[field]) }}</div>
                </template>
            </Column>
            <Column field="discount" header="Giảm giá">
                <template #body="{ data, field }">
                    <template v-if="!props.isClient">
                        <InputGroup>
                            <InputNumber
                                v-model="data[field]"
                                :min="0"
                                :max="data['discountType'] == 'P' ? 100 : undefined"
                                placeholder="0"
                                pt:input:root:class="w-20remd"
                                class="w-12rem"
                            />
                            <Dropdown
                                v-model="data['discountType']"
                                class="w-5rem"
                                :options="discountTypeOptions"
                                option-label="label"
                                option-value="value"
                                pt:trigger:class="hidden"
                                style="margin-left: -1px"
                                @change="
                                    () => {
                                        data[field] = 0;
                                    }
                                "
                            >
                                <!-- <template #value="slotProps">
                                    <i
                                        v-if="slotProps.value == 'C'"
                                        class="fa-solid fa-money-bill-wave"
                                    ></i>
                                    <i
                                        v-if="slotProps.value == 'P'"
                                        class="fa-solid fa-percent"
                                    ></i>
                                </template> -->
                            </Dropdown>
                        </InputGroup>
                    </template>
                    <div v-else class="text-right">{{ fnum(data[field]) }}</div>
                </template>
            </Column>
            <Column
                :header="`Đơn giá sau giảm (${poStore.model.currency})`"
                class="text-right"
            >
                <template #body="sp">
                    <template v-if="sp.data['discountType'] == 'P'">
                        {{
                            fnum(sp.data.price - (sp.data.price * sp.data.discount) / 100)
                        }}
                    </template>
                    <template v-else-if="sp.data['discountType'] == 'C'">
                        {{ fnum(sp.data.price - sp.data.discount) }}
                    </template>
                    <template v-else>0</template>
                </template>
            </Column>
            <Column field="vat" header="Thuế suất (%)">
                <template #body="{ data, field }">
                    <InputNumber
                        v-if="!props.isClient"
                        v-model="data[field]"
                        :min="0"
                        :max="100"
                        suffix=" %"
                        showButtons
                        pt:input:root:class="w-7rem"
                    />
                    <div v-else class="text-right">{{ fnum(data[field]) }}</div>
                </template>
            </Column>
            <Column
                :header="`Thành tiền trước thuế (${poStore.model.currency})`"
                class="text-right"
            >
                <template #body="sp">
                    <!-- {{
                        fnum(
                            (sp.data.price - (sp.data.price * sp.data.discount) / 100) *
                                sp.data.quantity
                        )
                    }} -->
                    <template v-if="sp.data['discountType'] == 'P'">
                        {{
                            fnum(
                                (sp.data.price -
                                    (sp.data.price * sp.data.discount) / 100) *
                                    sp.data.quantity
                            )
                        }}
                    </template>
                    <template v-else-if="sp.data['discountType'] == 'C'">
                        {{ fnum((sp.data.price - sp.data.discount) * sp.data.quantity) }}
                    </template>
                    <template v-else>0</template>
                </template>
            </Column>
            <Column field="paymentMethodCode" header="Phương thức thanh toán">
                <template #body="{ data, field }">
                    <Dropdown
                        v-model="data[field]"
                        optionValue="value"
                        optionLabel="label"
                        :options="paymentOptions"
                        class="w-full"
                    ></Dropdown>
                </template>
            </Column>
            <template #header v-if="selection.length && false">
                <div class="flex justify-content-end gap-2">
                    <Button
                        size="small"
                        label="Nhân bản"
                        :disabled="selection.length < 1"
                    />
                    <Button
                        @click="changePaymentMethodDialogRef?.open()"
                        size="small"
                        label="Phương thức thanh toán"
                        :disabled="selection.length < 1"
                    />
                    <Button
                        @click="onClickRemoveRow"
                        size="small"
                        label="Xóa"
                        severity="danger"
                    />
                </div>
            </template>
            <template #empty>
                <div class="my-7 py-7"></div>
            </template>
            <template #footer>
                <div class="flex justify-content-between">
                    <div class="flex gap-2">
                        <ProductSelector
                            v-if="poStore.model.cardId"
                            label="Thêm sản phẩm"
                            @confirm="onSelectProduct"
                            :customer="poStore.model._customer"
                            icon=""
                        ></ProductSelector>
                        <template v-if="selection.length">
                            <div class="flex justify-content-end gap-2">
                                <Divider layout="vertical" class="mx-1"></Divider>
                                <Button
                                    size="small"
                                    label="Nhân bản"
                                    :disabled="selection.length < 1"
                                    severity="help"
                                />
                                <Button
                                    @click="changePaymentMethodDialogRef?.open()"
                                    size="small"
                                    label="Phương thức thanh toán"
                                    :disabled="selection.length < 1"
                                    severity="info"
                                />
                                <Button
                                    @click="onClickRemoveRow"
                                    size="small"
                                    label="Xóa"
                                    severity="danger"
                                />
                            </div>
                        </template>
                    </div>
                    <div class="flex align-items-center gap-2">
                        <span style="line-height: 33px"
                            >Tổng sản lượng
                            {{ fnum(poStore.model.getTotalVolumn()) }} lít</span
                        >
                        <Button
                            outlined
                            @click="volumnDialogRef?.open()"
                            label="Xem chi tiết"
                            text
                            severity="info"
                            v-if="poStore.model.itemDetail.length > 0"
                        />
                    </div>
                </div>
            </template>
        </DataTable>
        <VolumnDialog ref="volumnDialogRef"></VolumnDialog>
        <ChangePaymentMethodDialog
            v-model:selected-item="selection"
            ref="changePaymentMethodDialogRef"
        />
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { ItemDetail } from "../types/entities";
import { ItemMasterData } from "../types/item.type";
import { fnum } from "../script";
//Components
import ChangePaymentMethodDialog from "../dialogs/ChangePaymentMethodDialog.vue";
const changePaymentMethodDialogRef = ref<InstanceType<
    typeof ChangePaymentMethodDialog
> | null>(null);
import VolumnDialog from "../dialogs/VolumnDialog.vue";
const volumnDialogRef = ref<InstanceType<typeof VolumnDialog> | null>(null);

import { usePoStore } from "../store/purchaseStore.store";
const poStore = usePoStore();

const props = defineProps({
    isClient: {
        type: Boolean,
        default: false,
    },
});

const selection = ref<ItemDetail[]>([]);

const onClickRemoveRow = () => {
    poStore.model.itemDetail = poStore.model.itemDetail.filter((item) => {
        return !selection.value.map((p) => p.itemId).includes(item.itemId);
    });
    selection.value = [];
};

const onSelectProduct = (event: ItemMasterData[]) => {
    for (var item of event) {
        const existItem = poStore.model.itemDetail.find((p) => p.itemId == item.id);
        if (existItem) {
            existItem.quantity++;
        } else {
            poStore.model.itemDetail.push(new ItemDetail(item));
        }
    }
};

const discountTypeOptions = [
    { label: "%", value: "P" },
    { label: "Tiền", value: "C" },
];

const paymentOptions = [
    { label: "Thanh toán ngay", value: "PayNow" },
    { label: "Công nợ tín chấp", value: "PayCredit" },
    { label: "Công nợ bảo lãnh", value: "PayGuarantee" },
];

watch(
    () => poStore.model._customer?.id,
    () => {
        selection.value = [];
    },
    { deep: true }
);

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
