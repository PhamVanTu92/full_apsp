<template>
    <div class="">
        <div class="mb-3 mt-3">
            <div class="ml-3 mb-2 text-lg font-bold">Thông tin khách hàng</div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Tên công ty</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.cardName }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Tên khác</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.frgnName }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Email</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.email }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Số điện thoại</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.phone }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Địa chỉ</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getAddress() }}
                    </div>
                </div>
            </div>
        </div>
        <div class="mb-3 mt-5">
            <div class="flex justify-content-between mx-3">
                <div class="text-lg font-bold">Thông tin nhận hàng</div>
                <Button
                    @click="onClickUpdate(poStore.model.cardId, 'S')"
                    label="Thay đổi"
                    severity="secondary"
                />
            </div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Người nhận</div>
                    <div class="flex-grow-1">
                        {{
                            poStore.model._customer?.getCrD1Default("S")?.default?.person
                        }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Địa chỉ</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.fullAddress }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Số điện thoại</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.default?.phone }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">CCCD</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.default?.cccd }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Biển số xe</div>
                    <div class="flex-grow-1">
                        {{
                            poStore.model._customer?.getCrD1Default("S")?.default
                                ?.vehiclePlate
                        }}
                    </div>
                </div>
            </div>
        </div>
        <div class="mb-3 mt-5">
            <div class="flex justify-content-between mx-3">
                <div class="text-lg font-bold">Địa chỉ xuất hóa đơn</div>
                <Button
                    @click="onClickUpdate(poStore.model.cardId, 'B')"
                    label="Thay đổi"
                    severity="secondary"
                />
            </div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Người liên hệ</div>
                    <div class="flex-grow-1">
                        {{
                            poStore.model._customer?.getCrD1Default("B")?.default?.person
                        }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Email</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("B")?.default?.email }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">Địa chỉ</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("B")?.fullAddress }}
                    </div>
                </div>
            </div>
        </div>
        <CustomerAddress
            ref="customeAddressRef"
            @change-default="onChangeDefault"
        ></CustomerAddress>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { Customer, PurchaseOrder } from "../types/entities";
import CustomerAddress from "../../../../components/CustomeAddress/index.vue";
import { usePoStore } from "../store/purchaseStore.store";
import API from "@/api/api-main";

const poStore = usePoStore();

const onChangeDefault = (e: any) => {

    API.get(`customer/${poStore.model._customer.id}`).then((res) => {

        poStore.model._customer = new Customer(res.data);
    });
};

const customeAddressRef = ref<InstanceType<typeof CustomerAddress>>();

const onClickUpdate = (id: number, type: "S" | "B") => {
    customeAddressRef.value?.openDialog(id, type);
};

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
.line-field {
    border-bottom: 1px solid #e2e8f0;
    /* @apply border-1 border-300 text-red-500; */
}
</style>
