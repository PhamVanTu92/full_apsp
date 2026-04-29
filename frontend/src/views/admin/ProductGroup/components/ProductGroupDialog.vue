<template>
    <div>
        <Dialog
            v-model:visible="visible"
            class="w-10 md:w-9"
            style="max-width: 958px"
            modal
            :header="model.id ? headerEdit : headerCreate"
            @hide="onClickClose"
        >
            <div>
                <div class="">
                    <div class="flex flex-column field">
                        <label for="itmsGrpName" class="font-semibold">
                            {{ t('body.sampleRequest.customerGroup.group_name_label') }} <sup class="text-red-500">*</sup>
                        </label>
                        <InputText
                            v-model="model.itmsGrpName"
                            id="itmsGrpName"
                            type="text"
                            :invalid="errorMsg?.itmsGrpName ? true : false"
                        />
                        <small class="text-red-500">{{ errorMsg?.itmsGrpName }}</small>
                    </div>
                    <div class="">
                        <div class="font-semibold">{{ t('body.productManagement.add_new_button') }}</div>
                        <small class="">{{ t('body.sampleRequest.customerGroup.add_customers_instruction') }}</small>
                    </div>
                    <hr />
                    <div class="flex flex-column gap-3 mb-3">
                        <div>
                            <RadioButton
                                v-model="model.isSelected"
                                :value="true"
                                inputId="isSelectedTrue"
                                @change="onChangeTypeCond()"
                            />
                            <label for="isSelectedTrue" class="ml-3">
                                {{ t('body.sampleRequest.customerGroup.manual_selection_radio') }}
                            </label>
                        </div>
                        <div>
                            <RadioButton
                                v-model="model.isSelected"
                                :value="false"
                                inputId="isSelectedFalse"
                                @change="onChangeTypeCond()"
                            />
                            <label for="isSelectedFalse" class="ml-3">
                                {{ t('body.sampleRequest.customerGroup.auto_selection_radio') }}
                            </label>
                        </div>
                        <div class="flex" v-if="!model.isSelected">
                            <div>{{ t('body.OrderApproval.selectAll') }}</div>
                            <div class="flex flex-column gap-3 ml-3">
                                <div>
                                    <RadioButton
                                        v-model="model.isOneOfThem"
                                        :value="false"
                                        inputId="isOneOfThemFalse"
                                    />
                                    <label for="isOneOfThemFalse" class="ml-3">{{ t('body.OrderApproval.selectAll') }}</label>
                                </div>
                                <div>
                                    <RadioButton
                                        v-model="model.isOneOfThem"
                                        :value="true"
                                        inputId="isOneOfThemTrue"
                                    />
                                    <label for="isOneOfThemTrue" class="ml-3">{{ t('body.productManagement.selected_label') }}</label>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="mb-3">
                        <DataTable
                            v-if="!model.isSelected"
                            :value="model.conditionItemGroups"
                            :pt="{
                                thead: {
                                    class: 'hidden',
                                },
                            }"
                        >
                            <Column class="w-1rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column class="w-15rem">
                                <template #body="{ data }">
                                    <Dropdown
                                        v-model="data.typeCondition"
                                        :options="getTypeConditionOption(data.typeCondition)"
                                        optionLabel="label"
                                        optionValue="value"
                                        :placeholder="t('body.sampleRequest.customerGroup.select_product_group_placeholder')"
                                        class="w-15rem"
                                        :invalid="errorMsg.list && !data.typeCondition"
                                    />
                                </template>
                            </Column>
                            <Column class="w-10rem">
                                <template #body="{ data }">
                                    <Dropdown
                                        v-model="data.isEqual"
                                        :options="operatorOptions"
                                        optionLabel="label"
                                        optionValue="value"
                                        class="w-10rem"
                                    />
                                </template>
                            </Column>
                            <Column>
                                <template #body="{ data }">
                                    <MultiSelect
                                        v-model="data.condValues"
                                        :options="valueOptions[data.typeCondition] || []"
                                        optionLabel="name"
                                        :optionValue="getOptionValueProperty(data.typeCondition)"
                                        :placeholder="t('body.sampleRequest.customerGroup.select_customer_placeholder')"
                                        class="w-full"
                                        selectedItemsLabel="{0} lựa chọn"
                                        :maxSelectedLabels="1"
                                        :invalid="errorMsg.list && data.condValues.length < 1"
                                    />
                                </template>
                            </Column>
                            <Column class="w-1rem">
                                <template #body="{ index }">
                                    <Button
                                        text
                                        severity="danger"
                                        icon="pi pi-trash"
                                        :disabled="model.conditionItemGroups.length < 2"
                                        @click="onRemoveRow(model.conditionItemGroups, index)"
                                    />
                                </template>
                            </Column>
                            <template #footer>
                                <Button
                                    :disabled="model.conditionItemGroups.length >= TypeConditionOption.length"
                                    @click="onClickAddCond"
                                    :label="t('body.promotion.addPromotionCondition')"
                                    icon="pi pi-plus-circle"
                                    outlined
                                />
                            </template>
                        </DataTable>
                        <DataTable
                            v-else
                            :value="model.items"
                            resizableColumns
                            columnResizeMode="fit"
                            :class="{
                                'border-1 border-red-500': errorMsg?.items && model.items.length < 1,
                            }"
                            scrollable
                            scrollHeight="300px"
                            class="border-1 border-200 border-bottom-none"
                            stripedRows
                        >
                            <Column header="#" class="w-1rem">
                                <template #body="{ index }">
                                    {{ index + 1 }}
                                </template>
                            </Column>
                            <Column :header=" t('body.productManagement.image_column')" class="w-1rem">
                                <template #body="{ data }">
                                    <Image
                                        :src="data?.itM1?.[0]?.filePath || 'https://placehold.co/40'"
                                        width="40"
                                        height="40"
                                    ></Image>
                                </template>
                            </Column>
                            <Column :header="t('body.productManagement.product_code_column')" field="itemCode"> </Column>
                            <Column :header="t('body.productManagement.product_name_column')" field="itemName"> </Column>
                            <Column class="w-1rem">
                                <template #body="{ index }">
                                    <Button
                                        @click="onRemoveRow(model.items, index)"
                                        text
                                        severity="danger"
                                        icon="pi pi-trash"
                                    />
                                </template>
                            </Column>
                            <template #footer>
                                <div class="flex align-items-center justify-content-between gap-3">
                                    <ProductSelector
                                        outlined
                                        :label="t('body.sampleRequest.importPlan.add_product_button')"
                                        icon="pi pi-plus-circle"
                                        @confirm="onSelectProduct"
                                    />
                                    <span>{{ model.items.length }} {{t('body.sampleRequest.importPlan.product_tab')}}</span>
                                </div>
                            </template>
                            <template #empty>
                                <div class="text-center py-5 my-5 text-500 font-italic">
                                    {{ t('body.systemSetting.no_data_to_display') }}
                                </div>
                            </template>
                        </DataTable>
                        <div class="mt-2">
                            <small v-if="errorMsg?.items && model.items.length < 1" class="text-red-500">
                                {{ errorMsg?.items }}
                            </small>
                        </div>
                    </div>
                    <div class="flex flex-column field">
                        <label for="description" class="font-semibold">{{ t('body.productManagement.description_column') }}</label>
                        <Textarea v-model="model.description" class="w-full" :rows="3" id="description" />
                    </div>
                    <div class="flex gap-3 align-items-center">
                        <label for="status" class="font-semibold">{{ t('body.productManagement.status_column') }}</label>
                        <InputSwitch v-model="model.isActive" inputId="status" />
                    </div>
                </div>
            </div>
            <template #footer>
                <Button @click="onClickClose" :label="t('body.PurchaseRequestList.cancel_button')" severity="secondary" icon="pi pi-times" />
                <Button :loading="loading" @click="onClickSave" :label="t('body.systemSetting.save_button')" severity="primary" icon="pi pi-save" />
            </template>
        </Dialog>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import {
    IProductGroup,
    ProductGroup,
    TypeConditionOption,
    postProductGroup,
    getProductGroupById,
    updateProductGroup,
} from "../entities/ProductGroup";
import API from "@/api/api-main";
import { useToast } from "primevue/usetoast";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const visible = ref(false);
const toast = useToast();
const loading = ref(false);
const emits = defineEmits(["refresh-data"]);

const model = ref<IProductGroup>(new ProductGroup());
const errorMsg = ref<any>({});

const headerEdit = t('body.productManagement.product_group_title') + ' - ' + t('body.promotion.update_button', { default: 'Edit' });
const headerCreate = t('body.productManagement.product_group_title');

const onClickSave = () => {
    const vresult = model.value.validate();
    if (!vresult.result) {
        errorMsg.value = vresult.errors;
        return;
    }
    const payload = model.value.toPayload();
    const method = payload.id ? updateProductGroup : postProductGroup;
    loading.value = true;
    method(payload)
        .then((res) => {
            toast.add({
                severity: "success",
                summary: t('body.systemSetting.success_label'),
                detail: t('body.systemSetting.update_account_success_message'),
                life: 3000,
            });
            loading.value = false;
            visible.value = false;
            emits("refresh-data", res);
        })
        .catch((error) => {
            toast.add({
                severity: "error",
                summary: t('body.report.error_occurred_message'),
                detail: t('body.report.error_occurred_message'),
                life: 3000,
            });
            loading.value = false;
            console.error(error);
        });
};

const openDialog = (id?: number) => {
    visible.value = true;
    if (id) {
        model.value.id = id;
        getProductGroupById(id).then((res) => {
            model.value = new ProductGroup(res.data);
        });
    } else {
        model.value = new ProductGroup();
    }
};

const onClickClose = () => {
    visible.value = false;
    errorMsg.value = {};
};

const onClickAddCond = () => {
    model.value.AddCond();
};
interface ITypeCond {
    label: string;
    value: string;
}
const getTypeConditionOption = (select: string | null): Array<ITypeCond> => {
    let result: Array<ITypeCond> = [];
    result = TypeConditionOption.filter((el) => {
        if (el.value == select) {
            return true;
        }
        return !model.value.conditionItemGroups
            .map((row) => row.typeCondition)
            .includes(el.value);
    });
    return result;
};

const onRemoveRow = (array: Array<any>, index: number) => {
    array.splice(index, 1);
};

const onSelectProduct = (products: Array<any>) => {

    // logic check if item already selected => skip
    const prodctNotInList = products?.filter(
        (el) => !model.value.items.map((row) => row.id).includes(el.id)
    );
    model.value.items.push(...prodctNotInList);
};

const onChangeTypeCond = () => {
    model.value.items = [];
    model.value.conditionItemGroups = [];
    model.value.isOneOfThem = false;
    if (model.value.isSelected) {
    } else {
        onClickAddCond();
    }
};

const operatorOptions = [
    {
        label: "Bằng",
        value: true,
    },
    {
        label: "Không bằng",
        value: false,
    },
];

defineExpose({
    openDialog,
});

const getOptionValueProperty = (type: string | null): string => {
    let prop = ["product_applications", "product_group"].includes(type || "")
        ? "code"
        : "id";
    return prop;
};

const valueOptions = reactive({
    industry: [],
    brand: [],
    item_type: [],
    product_applications: [],
    product_group: [],
    packing: [],
});

const initialComponent = () => {
    // Ngành hàng
    API.get("industry/getall").then((res) => {
        valueOptions.industry = res.data;
    });
    // Ngành hàng
    API.get("brand/getall").then((res) => {
        valueOptions.brand = res.data;
    });
    // Loại sản phẩm
    API.get("ItemType/getall").then((res) => {
        valueOptions.item_type = res.data?.items;
    });
    // Ứng dụng
    API.get("product-applications").then((res) => {
        valueOptions.product_applications = res.data;
    });
    // Nhóm hàng hóa
    API.get("product-groups").then((res) => {
        valueOptions.product_group = res.data;
    });
    // Nhóm hàng hóa
    API.get("Packing/getall").then((res) => {
        valueOptions.packing = res.data;
    });
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
