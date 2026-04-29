<template>
    <div class="">
        <div class="mb-3 mt-3">
            <div class="ml-3 mb-2 text-lg font-bold">{{t('client.customer_info')}}</div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('Custom.company_name')}}</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.cardName }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('Custom.alternate_name')}}</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.frgnName }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.email_label')}}</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.email }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.phone_number_label')}}</div>
                    <div class="flex-grow-1">{{ poStore.model._customer?.phone }}</div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.invoice_address_label')}}</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getAddress() }}
                    </div>
                </div>
            </div>
        </div>
        <div class="mb-3 mt-5">
            <div class="flex justify-content-between mx-3">
                <div class="text-lg font-bold">{{t('Custom.shipping_info')}}</div>
                <Button
                    @click="onClickUpdate(poStore.model.cardId, 'S')"
                    :label="t('Custom.change')"
                    severity="secondary"
                />
            </div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('Custom.receiver_name')}}</div>
                    <div class="flex-grow-1">
                        {{
                            poStore.model._customer?.getCrD1Default("S")?.default?.person
                        }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.invoice_address_label')}}</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.fullAddress }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.phone_number_label')}}</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.default?.phone }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.cccd_number')}}</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("S")?.default?.cccd }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.licensePlate')}}</div>
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
                <div class="text-lg font-bold">{{t('client.billingAddress')}}</div>
                <Button
                    @click="onClickUpdate(poStore.model.cardId, 'B')"
                    :label="t('Custom.change')"
                    severity="secondary"
                />
            </div>
            <div class="grid m-0">
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.contact_person')}} 1</div>
                    <div class="flex-grow-1">
                        {{
                            poStore.model._customer?.getCrD1Default("B")?.default?.person
                        }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.email_label')}}</div>
                    <div class="flex-grow-1">
                        {{ poStore.model._customer?.getCrD1Default("B")?.default?.email }}
                    </div>
                </div>
                <div class="flex col-12 line-field">
                    <div class="w-20rem">{{t('client.invoice_address_label')}}</div>
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
import { ref, onMounted } from "vue";
import { Customer } from "../types/entities";
import CustomerAddress from "@/components/CustomeAddress/index.vue";
import { usePoStore } from "../store/purchaseStore.store";
import API from "@/api/api-main";
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

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
