<template>
    <div class="grid">
        <div class="col-12 md:col-4">
            <div class="text-lg font-bold my-2">{{ t('client.customer_info') }}</div>
            <hr />
            <div class="field">
                <label>{{ t('client.customerName') }}</label>
                <p>{{ odStore.order?.cardName || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.tax_code') }}</label>
                <p>{{ odStore.customer?.licTradNum || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.phoneNumber') }}</label>
                <p>{{ odStore.customer?.phone || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.email') }}</label>
                <p>{{ odStore.customer?.email || "-" }}</p>
            </div>
        </div>
        <div class="col-12 md:col-4">
            <div class="flex justify-content-between align-items-center mb-2">
                <div class="text-lg font-bold my-2">{{ t('client.delivery_info') }}</div>
                <Button
                    v-if="!(['DGH','DHT'] as any).includes(odStore.order?.status)"
                    icon="pi pi-pencil"
                    text
                    @click="addressUpdateRef?.open('S')"
                />
            </div>
            <hr class="mt-0" />
            <div class="field">
                <label>{{ t('client.contact_person') }}</label>
                <p>{{ shipAddress?.person || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.phoneNumber') }}</label>
                <p>{{ shipAddress?.phone || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.citizen_id') }}</label>
                <p>{{ shipAddress?.cccd || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.vehicle_plate') }}</label>
                <p>{{ shipAddress?.vehiclePlate || "-" }}</p>
            </div>
            <div v-if="shipAddress?.email" class="field">
                <label>{{ t('client.email') }}</label>
                <p>{{ shipAddress?.email || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.delivery_address') }}</label>
                <p>{{ shipAddress?.fullAddress || "-" }}</p>
            </div>
        </div>
        <div class="col-12 md:col-4">
            <div class="flex justify-content-between align-items-center mb-2">
                <div class="text-lg font-bold my-2">{{ t('client.invoice_info') }}</div>
                <Button
                    v-if="false"
                    icon="pi pi-pencil"
                    text
                    @click="addressUpdateRef?.open('B')"
                />
            </div>
            <hr class="mt-0" />
            <div class="field">
                <label>{{ t('client.contact_person') }}</label>
                <p>{{ billAddress?.person || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.phoneNumber') }}</label>
                <p>{{ billAddress?.phone || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.email') }}</label>
                <p>{{ billAddress?.email || "-" }}</p>
            </div>
            <div class="field">
                <label>{{ t('client.address') }}</label>
                <p>{{ billAddress?.fullAddress || "-" }}</p>
            </div>
        </div>
        <AddressUpdate ref="addressUpdateRef"></AddressUpdate>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useOrderDetailStore } from "../store/orderDetailNET";
import AddressUpdate from "../dialogs/AddressUpdate.vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
const odStore = useOrderDetailStore();

const addressUpdateRef = ref<InstanceType<typeof AddressUpdate>>();

const billAddress = computed(() => {
    const adrs = odStore.order?.address.find((x) => x.type == "B");
    const address = {} as typeof adrs & { fullAddress: string };
    if (adrs) {
        Object.assign(address, adrs);
    }
    address.fullAddress = [address.address, address.locationName, address.areaName]
        .filter((x) => x)
        .join(", ");
    return address;
});

const shipAddress = computed(() => {
    const adrs = odStore.order?.address.find((x) => x.type == "S");
    const address = {} as typeof adrs & { fullAddress: string };
    if (adrs) {
        Object.assign(address, adrs);
    }
    address.fullAddress = [address.address, address.locationName, address.areaName]
        .filter((x) => x)
        .join(", ");
    return address;
});

const initialComponent = () => {
    // code here
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped>
label {
    font-weight: 700;
}
</style>
