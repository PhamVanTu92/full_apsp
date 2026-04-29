<template>
    <div class="flex justify-content-between mb-3">
        <div>
            <h4 class="m-0 font-semibold">{{ t('body.sampleRequest.sampleProposal.create_new_sample_delivery_request') }}</h4>
        </div>
        <div class="flex gap-3">
            <Button :label="t('body.sampleRequest.sampleProposal.back_button')" @click="goBack()" severity="secondary"/>
            <Button
                @click="createSample('NHAP')"
                :label="t('body.sampleRequest.sampleProposal.save_draft_button')"
                severity="info"
            />
            <Button @click="createSample()" :label="t('body.sampleRequest.sampleProposal.send_for_approval_button')"/>
        </div>
    </div>

    <div class="flex justify-content-between card p-3">
        <div class="col-2">
            <div class="flex flex-column gap-2 mb-3">
                <div>{{ t('body.sampleRequest.sampleProposal.request_code_label') }}</div>
                <InputText
                    v-model="payload.invoiceCode"
                    type="text"
                    disabled
                    :placeholder="t('body.sampleRequest.sampleProposal.auto_generated_code')"
                />
            </div>
            <div class="flex gap-2">
                <div class="py-3">
                    <span>{{ t('body.sampleRequest.sampleProposal.created_date_label') }}</span>
                    <span class="ml-3">{{ format(payload.docDate, "dd/MM/yyyy") }}</span>
                </div>
            </div>
        </div>
        <div class="col flex gap-3 flex-column justify-content-end">
            <div class="flex flex-wrap gap-2 mb-3">
                <div>{{ t('body.sampleRequest.sampleProposal.customer_label') }}</div>
                <div class="flex align-items-center">
                    <RadioButton
                        v-model="isNewCustomer"
                        inputId="ingredient1"
                        :name="t('body.sampleRequest.sampleProposal.new_customer_radio')"
                        :value="true"
                        @change="
                            () => {
                                payload.internalCust = false;
                                payload.externalCust = true;
                            }
                        "
                    />
                    <label for="ingredient1" class="ml-2"> {{ t('body.sampleRequest.sampleProposal.new_customer_radio') }}</label>
                </div>
                <div class="flex align-items-center">
                    <RadioButton
                        v-model="isNewCustomer"
                        inputId="ingredient2"
                        :name="t('body.sampleRequest.sampleProposal.existing_customer_radio')"
                        :value="false"
                        @change="
                            () => {
                                payload.internalCust = true;
                                payload.externalCust = false;
                            }
                        "
                    />
                    <label for="ingredient2" class="ml-2">{{ t('body.sampleRequest.sampleProposal.existing_customer_radio') }}</label>
                </div>
            </div>
            <div class="flex gap-3">
                <div v-if="isNewCustomer" class="flex gap-3">
                    <div class="flex flex-column gap-2">
                        <label>
                            {{ t('body.sampleRequest.sampleProposal.customer_name_label') }}
                            <sup class="text-red-500 font-bold">*</sup>
                        </label>
                        <InputText
                            type="text"
                            v-model="payload.cardName"
                            class="w-30rem"
                        />
                    </div>
                    <div class="flex flex-column gap-2">
                        <label>
                            {{ t('body.sampleRequest.sampleProposal.customer_type_label') }}
                            <sup class="text-red-500 font-bold">*</sup>
                        </label>
                        <Dropdown
                            v-model="payload.cardType"
                            :options="listCustomer"
                            optionLabel="name"
                            optionValue="code"
                            :placeholder="t('body.sampleRequest.sampleProposal.select_customer_type_placeholder')"
                            class="w-20rem"
                        />
                    </div>
                </div>
                <div v-else class="flex flex-column gap-2 w-30rem">
                    <label>
                        {{ t('body.sampleRequest.sampleProposal.customer_label') }}
                        <sup class="text-red-500 font-bold">*</sup>
                    </label>
                    <CustomerSelector
                        @item-select="onSelectCustomer"
                        :placeholder="t('body.OrderList.select_customer_placeholder')"
                        v-model="payload.cardId"
                    ></CustomerSelector>
                </div>
                <div class="flex flex-column gap-2 justify-content-end">
                    <label>{{ t('body.sampleRequest.sampleProposal.creator_label') }}</label>
                    <InputText :value="userInfo?.user?.fullName" readonly></InputText>
                </div>
            </div>
        </div>
    </div>

    <div class="card">
        <DataTable :value="product" showGridlines scrollable scrollHeight="535px">
            <Column header="#" class="w-1rem">
                <template #body="{ index }">
                    {{ index + 1 }}
                </template>
            </Column>
            <Column :header="t('body.sampleRequest.sampleProposal.product_code_column')" field="itemCode" style="width: 10rem"></Column>
            <Column :header="t('body.sampleRequest.sampleProposal.product_name_column')" field="itemName"></Column>
            <Column :header="t('body.sampleRequest.sampleProposal.quantity_liters_column')" field="quantity" style="width: 10rem">
                <template #body="{ data }">
                    <InputNumber
                        v-model="data.quantity"
                        inputId="withoutgrouping"
                        :useGrouping="false"
                        :min="1"
                        pt:input:root:class="w-10rem text-right"
                    />
                </template>
            </Column>
            <Column style="width: 65px">
                <template #body="{ index }">
                    <Button
                        @click="onClickRemoveLine(index)"
                        icon="pi pi-trash"
                        text
                        severity="danger"
                    />
                </template>
            </Column>
            <ColumnGroup type="footer">
                <Row>
                    <Column :colspan="3">
                        <template #footer>
                            <ProductSelector
                                :label="t('body.sampleRequest.sampleProposal.choose_product_button')"
                                @confirm="onSelectedProduct($event)"
                            ></ProductSelector>
                        </template>
                    </Column>
                    <Column :footer="totalLitter" class="text-right font-normal"></Column>
                    <Column></Column>
                </Row>
            </ColumnGroup>
            <template #empty>
                <div class="p-5 m-5 text-center text-500 font-italic">
                    {{ t('body.sampleRequest.sampleProposal.sample_products_note') }}
                </div>
            </template>
        </DataTable>
    </div>
    <Loading v-if="loading"></Loading>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from "vue";
import API from "@/api/api-main";
import { useGlobal } from "../../../services/useGlobal";
import { useMeStore } from "../../../Pinia/me";
import { format } from "date-fns";
import { useI18n } from "vue-i18n";
const { t } = useI18n();

const { toast, FunctionGlobal } = useGlobal();
const isNewCustomer = ref(true);
const loading = ref(false);
const product = ref<
    {
        id?: number;
        fatherId?: number;
        itemId: number;
        itemCode: string;
        itemName: string;
        quantity: number | null;
        result: string | null;
    }[]
>([]);
const userStore = useMeStore();

const totalLitter = computed(() => {
    let sum = product.value.reduce((sum, item) => {
        return item.quantity ? sum + item.quantity : sum;
    }, 0);
    return Intl.NumberFormat().format(sum);
});

const onClickRemoveLine = (index: number) => {
    product.value.splice(index, 1);
};

const payload = ref({
    id: 0,
    invoiceCode: "",
    internalCust: false,
    externalCust: true,
    cardId: 0,
    cardCode: "",
    cardName: "",
    cardType: "",
    status: "",
    docDate: new Date(),
    rfS1: [] as {
        id?: number;
        fatherId?: number;
        itemId: number;
        itemCode: string;
        itemName: string;
        quantity: number | null;
        result: string | null;
    }[],
    userId: 0,
    userName: "",
});

const listCustomer = [
    {
        name: t('body.sampleRequest.sampleProposal.customer_type_industrial'),
        code: "IN",
    },
    {
        name: t('body.sampleRequest.sampleProposal.customer_type_export'),
        code: "EX",
    },
    {
        name: t('body.home.distributors'),
        code: "DI",
    },
];

const createSample = async (stt: any = null) => {
    payload.value.rfS1 = product.value;
    if (isNewCustomer.value) {
        if (!payload.value.cardName) {
            FunctionGlobal.$notify("E", t('body.sampleRequest.sampleProposal.validate_customer_name'), toast);
            return;
        } else if (!payload.value.cardType) {
            FunctionGlobal.$notify("E", t('body.sampleRequest.sampleProposal.validate_customer_type'), toast);
            return;
        }
    } else {
        if (!payload.value.cardId) {
            FunctionGlobal.$notify("E", t('body.sampleRequest.sampleProposal.validate_select_customer'), toast);
            return;
        }
    }
    payload.value.rfS1.map((item) => item.quantity);
    try {
        if (stt) {
            payload.value.status = stt;
        }
        const response = await API.add("RequestOfExample/Add", payload.value);
        if (response.status === 200) {
            FunctionGlobal.$notify("S", "Cập nhật thành công!", toast);
            goBack();
        }
    } catch (error) {
        FunctionGlobal.$notify("E", "Cập nhật thất bại!", toast);
    }
};

const onSelectedProduct = (e: any) => {
    e.forEach((item: any) => {
        // Kiểm tra nếu itemId đã có trong product.value
        const existingProduct = product.value.find((p) => p.itemId === item.id);

        if (existingProduct) {
            if (existingProduct.quantity) existingProduct.quantity += 1;
        } else {
            product.value.push({
                itemId: item.id,
                itemCode: item.itemCode,
                itemName: item.itemName,
                quantity: 1,
                result: null,
            });
        }
    });
};

const goBack = () => {
    window.history.back();
};

const onSelectCustomer = (val: any) => {

    payload.value.cardCode = val.cardCode;
    payload.value.cardName = val.cardName;
};
const userInfo = ref();
onMounted(async () => {
    userInfo.value = await userStore.getMe();
});
</script>
