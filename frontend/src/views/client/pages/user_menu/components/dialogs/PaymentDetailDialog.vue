<template>
    <div>
        <Dialog
            v-model:visible="visible"
            header="Chi tiết thanh toán"
            style="max-width: 50rem; width: 95vw"
            modal
        >
            <div class="flex flex-column gap-3">
                <div class="title">Thưởng sản lượng cam kết</div>
                <div class="mx-5 flex flex-column gap-3">
                    <div class="flex justify-content-between">
                        <span>Thưởng sản lượng Quý xxx</span>
                        <span>-</span>
                    </div>
                    <div class="flex justify-content-between">
                        <span>Thưởng xxx tháng</span>
                        <span>-</span>
                    </div>
                    <hr class="my-0" />
                    <div class="flex justify-content-between">
                        <span>Tổng thưởng sản lượng</span>
                        <span class="w-15rem text-right">-</span>
                    </div>
                </div>
                <div class="title">Thưởng thanh toán ngay</div>
                <div class="mx-5 flex flex-column gap-3">
                    <div class="flex justify-content-between">
                        <div>
                            <div>Thưởng thanh toán ngay</div>
                            <div class="text-sm text-gray-500">
                                Giảm {{ fnum(poStore.orderSummary.bonusPercent * 100) }}%
                                trên tổng giá niêm yết
                            </div>
                        </div>
                        <span
                            >{{
                                fnum(
                                    poStore.orderSummary.bonusAmount,
                                    2,
                                    ` ${poStore.model.currency}`
                                )
                            }}
                        </span>
                    </div>
                </div>
                <div class="title">Giá trị đơn hàng sau CK</div>
                <div class="mx-5 flex flex-column gap-3">
                    <div class="flex justify-content-between">
                        <span>Thanh toán ngay </span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.totalPayNowBeforeVat,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <div class="flex justify-content-between">
                        <span>Tín chấp</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.totalDebtBeforeVat,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <div class="flex justify-content-between">
                        <span>Bảo lãnh</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.totalDebtGuaranteeBeforeVat,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <hr class="my-0" />
                    <div class="flex justify-content-between">
                        <span>Tổng</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.totalPayNowBeforeVat +
                                    poStore.orderSummary.totalDebtBeforeVat +
                                    poStore.orderSummary.totalDebtGuaranteeBeforeVat,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                </div>
                <div class="title">Tổng tiền thuế</div>
                <div class="mx-5 flex flex-column gap-3">
                    <div class="flex justify-content-between">
                        <span>Thanh toán ngay</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.paynowVATAmount,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <div class="flex justify-content-between">
                        <span>Tín chấp</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.debtVATAmount,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <div class="flex justify-content-between">
                        <span>Bảo lãnh</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.debtGuaranteeVATAmount,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                    <hr class="my-0" />
                    <div class="flex justify-content-between">
                        <span>Tổng</span>
                        <span class="w-15rem text-right">{{
                            fnum(
                                poStore.orderSummary.paynowVATAmount +
                                    poStore.orderSummary.debtVATAmount +
                                    poStore.orderSummary.debtGuaranteeVATAmount,
                                2,
                                ` ${poStore.model.currency}`
                            )
                        }}</span>
                    </div>
                </div>
            </div>
            <template #footer>
                <Button
                    @click="visible = false"
                    label="Đóng"
                    severity="secondary"
                />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref,  onMounted } from "vue";
import { usePoStore } from "../store/purchaseStore.store";
import { fnum } from "../types/script";

const poStore = usePoStore();
const visible = ref(false);

const open = () => {
    visible.value = true;
};

defineExpose({
    open,
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.title {
    border-left: 5px solid var(--primary-color);
    padding: 6px 0px 6px 10px;
    font-size: medium;
    background: rgba(207, 241, 202, 0.575);
}
</style>
