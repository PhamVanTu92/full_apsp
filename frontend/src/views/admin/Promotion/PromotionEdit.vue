<script setup>
import { provide } from 'vue';
import { usePromotionEdit } from './composables/usePromotionEdit';
import ProductDialog from './components/ProductDialog.vue';
import PromotionInfoCard from './components/PromotionInfoCard.vue';
import PromotionSummaryCard from './components/PromotionSummaryCard.vue';
import PromotionLineBlock from './components/PromotionLineBlock.vue';
import PromotionExceptionBlock from './components/PromotionExceptionBlock.vue';

const {
    // State
    payload,
    loading,
    submited,
    visible,
    isFilterPro,
    ItemData,
    ItemType,
    selectedItemType,
    Hierarchy,
    // Options
    optionAddValueType,
    optionAddType,
    optionFollowBy,
    optionFollowBy2,
    optionSubType,
    optionPromotionType,
    comparisonOptions,
    // Router
    router,
    // Functions
    getDataIndustry,
    setUnitProduct,
    addCondition,
    addRows,
    addForm,
    AddFormHasException,
    AddConditionQuantity,
    removeCondition,
    removeConditionGroup,
    removeForm,
    RemoveItem,
    RemoveItemProduct,
    CloneCondition,
    CloneConditionSub,
    changeCondition,
    changConditonTow,
    checkSubType,
    checkSubTypeHasException,
    checkByIndex,
    getFirstIndex,
    findMaxIngroup,
    ChangeSubFrom,
    changeFormGroup,
    changeHasEx,
    renderLabel,
    changerBrand,
    changerIndustry,
    openSelectProduct,
    updateSelectedProduct,
    confirmDataProduct,
    confirmDataTypeItem,
    SavePromotion,
    t
} = usePromotionEdit();

// Provide helper functions for child components
provide('checkSubType', checkSubType);
provide('checkSubTypeHasException', checkSubTypeHasException);
provide('checkByIndex', checkByIndex);
provide('getFirstIndex', getFirstIndex);
provide('findMaxIngroup', findMaxIngroup);
provide('renderLabel', renderLabel);
provide('setUnitProduct', setUnitProduct);
provide('getDataIndustry', getDataIndustry);
</script>
<template>
    <div class="flex justify-content-between align-items-center mb-2 sticky top-0">
        <h4 class="font-bold m-0">
            {{ payload.id ? t('body.promotion.update_promotion_page_title') :
                t('body.promotion.create_promotion_page_title') }}
        </h4>
        <div class="flex gap-2">
            <Button @click="router.back()" :label="t('body.promotion.back_button')" icon="pi pi-arrow-left"
                severity="secondary" />
            <Button label="Lưu" icon="pi pi-save" @click="SavePromotion()" />
        </div>
    </div>
    <div class="grid">
        <div class="col-8">
            <PromotionInfoCard :payload="payload" :submited="submited" :loading="loading" />
        </div>
        <div class="col-4">
            <PromotionSummaryCard :payload="payload" />
        </div>
        <div class="col-12">
            <PromotionLineBlock :payload="payload" :submited="submited" :Hierarchy="Hierarchy"
                :optionSubType="optionSubType" :optionPromotionType="optionPromotionType"
                :optionFollowBy="optionFollowBy" :optionFollowBy2="optionFollowBy2"
                :optionAddType="optionAddType" :optionAddValueType="optionAddValueType"
                :comparisonOptions="comparisonOptions" @addCondition="addCondition" @addRows="addRows"
                @addForm="addForm" @removeForm="removeForm" @removeCondition="removeCondition"
                @removeConditionGroup="removeConditionGroup" @openSelectProduct="openSelectProduct"
                @CloneCondition="CloneCondition" @CloneConditionSub="CloneConditionSub"
                @AddConditionQuantity="AddConditionQuantity" @RemoveItemProduct="RemoveItemProduct"
                @changeCondition="changeCondition" @changConditonTow="changConditonTow"
                @ChangeSubFrom="ChangeSubFrom" @changeFormGroup="changeFormGroup"
                @changerBrand="changerBrand" @changerIndustry="changerIndustry" />

            <!-- Điều kiện ngoại lệ -->
            <PromotionExceptionBlock :payload="payload" :submited="submited" :optionFollowBy="optionFollowBy"
                :optionFollowBy2="optionFollowBy2" :optionAddType="optionAddType"
                :optionAddValueType="optionAddValueType" @changeHasEx="changeHasEx"
                @addCondition="addCondition" @AddFormHasException="AddFormHasException"
                @removeForm="removeForm" @removeCondition="removeCondition"
                @removeConditionGroup="removeConditionGroup" @openSelectProduct="openSelectProduct"
                @CloneCondition="CloneCondition" @AddConditionQuantity="AddConditionQuantity"
                @ChangeSubFrom="ChangeSubFrom" @changeFormGroup="changeFormGroup"
                @changConditonTow="changConditonTow" />
        </div>
    </div>
    <!-- Chọn sản phẩm -->
    <ProductDialog v-model="visible" :is-filter-pro="isFilterPro" :item-data="ItemData" :item-type="ItemType"
        :selected-item-type="selectedItemType" @confirm-product="confirmDataProduct"
        @confirm-item-type="confirmDataTypeItem" @update-product="updateSelectedProduct" @remove-item="RemoveItem" />
    <Loading v-if="loading"></Loading>
</template>
<style scoped>
@import url('./style.css');
</style>
