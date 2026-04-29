<script setup>
import { groupBy } from 'lodash';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const props = defineProps({
    payload: { type: Object, required: true },
    submited: { type: Boolean, default: false },
    Hierarchy: { type: Array, default: () => [] },
    optionSubType: { type: Array, required: true },
    optionPromotionType: { type: Array, required: true },
    optionFollowBy: { type: Array, required: true },
    optionFollowBy2: { type: Array, required: true },
    optionAddType: { type: Array, required: true },
    optionAddValueType: { type: Array, required: true },
    comparisonOptions: { type: Array, required: true },
});

const emit = defineEmits([
    'addCondition', 'addRows', 'addForm', 'removeForm',
    'removeCondition', 'removeConditionGroup',
    'openSelectProduct', 'CloneCondition', 'CloneConditionSub',
    'AddConditionQuantity', 'RemoveItemProduct',
    'changeCondition', 'changConditonTow',
    'ChangeSubFrom', 'changeFormGroup',
    'changerBrand', 'changerIndustry',
]);

// Proxy functions from parent composable
const checkSubType = inject('checkSubType');
const checkByIndex = inject('checkByIndex');
const getFirstIndex = inject('getFirstIndex');
const findMaxIngroup = inject('findMaxIngroup');
const renderLabel = inject('renderLabel');
const setUnitProduct = inject('setUnitProduct');
const getDataIndustry = inject('getDataIndustry');

import { inject } from 'vue';
</script>
<template>
    <div class="card">
        <h5 class="m-0 font-bold">{{ t('body.promotion.promotion') }}</h5>
        <div class="flex gap-3 mt-3 w-6">
            <div class="flex flex-column gap-2 w-full">
                <h6 class="m-0 font-bold">{{ t('body.productManagement.brand') }} <sup
                        class="text-red-500">*</sup></h6>
                <MultiSelect :options="Hierarchy" optionLabel="brandName" optionValue="brandId" filter
                    resetFilterOnHide :invalid="submited && !payload.promotionBrand.length" class="w-full"
                    :placeholder="t('client.input_brand')" v-model="payload.promotionBrand"
                    @change="emit('changerBrand')" />
            </div>
            <div class="flex flex-column gap-2 w-full">
                <h6 class="m-0 font-bold">{{ t('body.productManagement.category') }} <sup
                        class="text-red-500">*</sup></h6>
                <MultiSelect :options="getDataIndustry(payload.promotionBrand)" optionLabel="industryName"
                    optionValue="industryId" class="w-full" filter resetFilterOnHide
                    :invalid="submited && !payload.promotionIndustry.length"
                    :placeholder="t('body.sampleRequest.customer.select_category')"
                    :disabled="!payload.promotionBrand?.length" v-model="payload.promotionIndustry"
                    @change="emit('changerIndustry')" />
            </div>
            <div class="flex flex-column gap-2 w-full">
                <h6 class="m-0 font-bold">{{ t('body.promotion.form_column') }}</h6>
                <Dropdown class="w-full" v-model="payload.promotionType" :options="optionPromotionType"
                    optionLabel="name" optionValue="code" />
            </div>
        </div>
        <hr />
        <div class="overflow-x-auto pb-2">
            <div style="min-width: 1400px">
                <div class="grid m-0 mb-3"
                    v-for="(item, i) in payload.promotionLine.filter((e) => e.status != 'D' && !e.HasExceptionBlock)"
                    :key="item">
                    <div class="col-1 flex align-items-center p-0">
                        <SelectButton v-if="!i" class="border-1 border-300 border-round z-2" v-model="item.cond"
                            :options="comparisonOptions" optionValue="value" optionLabel="label"
                            aria-labelledby="basic" :allowEmpty="false" />
                    </div>

                    <div class="col-11 p-0">
                        <div class="card line z-1 py-3 bg-gray-200" :class="{
                            'line-bottom': i == 0 || payload.promotionLine.filter((l) => l.status != 'D' && !l.HasExceptionBlock).length < 2,
                            'line-straight none-bottom': i >= 1
                        }">
                            <div class="card">
                                <div class="flex justify-content-between">
                                    <div class="flex gap-2 align-items-center w-9">
                                        <h6 class="m-0 font-bold">{{ t('body.promotion.form_column') }}</h6>
                                        <Dropdown class="w-3"
                                            :options="optionSubType.filter((e) => checkSubType(item.subType).includes(e.code))"
                                            optionLabel="name" optionValue="code"
                                            @change="emit('ChangeSubFrom', item.promotionLineSub, item.subType)"
                                            v-model="item.subType" />
                                        <Checkbox v-model="item.addAccumulate" binary />
                                        <span>{{ t('client.product_not_accumulated') }}</span>
                                    </div>
                                    <Button :label="t('body.OrderList.delete')" outlined size="small"
                                        severity="danger" @click="emit('removeForm', payload, item)" />
                                </div>
                                <hr />
                                <div class="mb-2 flex justify-content-end">
                                    <Button :label="t('body.promotion.add_promotion_button')" icon="pi pi-plus"
                                        size="small" severity="secondary"
                                        @click="emit('addCondition', item.promotionLineSub, item.subType)"
                                        v-if="!checkByIndex(item.promotionLineSub) || item.subType == 1" />
                                </div>
                                <p class="m-0 font-bold mb-3">{{ t('body.promotion.form_column') }}</p>
                                <!-- SubType 1 -->
                                <div v-if="item.subType == 1">
                                    <div class="card p-4 border-round-md mb-5"
                                        v-for="(sub, subIndex) in item.promotionLineSub.filter((e) => e.status != 'D')"
                                        :key="sub">
                                        <div class="flex justify-content-between">
                                            <div class="flex flex-column gap-4">
                                                <div class="flex gap-2 align-items-center">
                                                    <span>Từ</span>
                                                    <InputNumber :min="1" placeholder="Số lượng"
                                                        v-model="sub.quantity" style="width: 70px;" />
                                                    <InputGroup style="width: 300px"
                                                        @click="emit('openSelectProduct', sub.promotionItemBuy, true)">
                                                        <InputText
                                                            :placeholder="t('body.promotion.selectProductOrType')"
                                                            :invalid="submited && sub.promotionItemBuy.length === 0"
                                                            :value="renderLabel(sub.promotionItemBuy)" />
                                                        <InputGroupAddon>
                                                            <i class="pi pi-list-check" />
                                                        </InputGroupAddon>
                                                    </InputGroup>
                                                    <MultiSelect filter
                                                        v-if="sub.promotionItemBuy[0]?.itemType == 'G'"
                                                        style="width: 250px" optionLabel="packingName"
                                                        optionValue="packingId"
                                                        :options="setUnitProduct(sub.promotionItemBuy, sub)"
                                                        v-model="sub.promotionUnit"
                                                        selectedItemsLabel="Đã chọn {0} quy cách bao bì"
                                                        :maxSelectedLabels="2" placeholder="Quy cách bao bì">
                                                    </MultiSelect>
                                                    <div class="flex gap-2 align-items-center">
                                                        <span>{{ t('body.promotion.get') }}</span>
                                                        <Dropdown :options="optionAddType" optionLabel="name"
                                                            optionValue="code" v-model="sub.addType"
                                                            style="width: 150px" />
                                                        <InputNumber :min="0"
                                                            :placeholder="t('body.promotion.buy')"
                                                            v-if="sub.addType == 'R'" v-model="sub.addBuy"
                                                            style="min-width: 70px;" />
                                                        <InputNumber :min="0" v-model="sub.addQty"
                                                            :placeholder="t('body.promotion.get')"
                                                            style="width: 70px;" />
                                                        <Checkbox v-model="sub.isSameType" binary @change="
                                                            emit('changConditonTow',
                                                                sub.promotionLineSubSub.filter((e) => e.status != 'D'),
                                                                sub.isSameType
                                                            )
                                                            " />
                                                        <span>{{ t('body.promotion.different_type') }}</span>
                                                        <Dropdown v-if="sub.isSameType" class="w-4"
                                                            :options="optionAddValueType" optionLabel="name"
                                                            optionValue="code" v-model="sub.addValueType" />
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="flex gap-2">
                                                <Button text icon="pi pi-plus-circle"
                                                    @click="emit('AddConditionQuantity', item, sub)"
                                                    v-if="item.promotionLineSub.filter((e) => !e.followBy).length == subIndex + 1" />
                                                <Button text icon="pi pi-clone" severity="info"
                                                    @click="emit('CloneCondition', item, sub)" />
                                                <Button text icon="pi pi-trash" severity="danger"
                                                    @click="emit('removeCondition', item, sub, subIndex)" />
                                            </div>
                                        </div>
                                        <div>
                                            <hr />
                                            <div class="mt-3">
                                                <div class="mb-2 flex justify-content-between">
                                                    <p class="m-0 font-bold">
                                                        {{ t('body.promotion.title') }}
                                                    </p>
                                                </div>
                                                <div class="grid m-0" v-for="(subSub, idxSub) in groupBy(
                                                    sub.promotionLineSubSub.filter((e) => e.status != 'D'),
                                                    'inGroup'
                                                )" :key="subSub">
                                                    <div class="col-1 w-11rem flex">
                                                        <div class="flex align-items-center">
                                                            <SelectButton :options="comparisonOptions"
                                                                v-model="subSub[0].cond"
                                                                class="border-1 border-300 border-round z-2"
                                                                optionValue="value" optionLabel="label"
                                                                aria-labelledby="basic" @change="
                                                                    emit('changeCondition',
                                                                        subSub,
                                                                        sub.promotionLineSubSub.filter((e) => e.status != 'D'),
                                                                        $event,
                                                                        sub.isSameType
                                                                    )
                                                                    " :allowEmpty="false" />
                                                        </div>
                                                    </div>

                                                    <div class="p-0 line-w-lg line col" :class="{
                                                        'line-bottom': idxSub == 1,
                                                        'line-bottom-no-left-before':
                                                            Object.keys(
                                                                groupBy(
                                                                    sub.promotionLineSubSub.filter((e) => e.status != 'D'),
                                                                    'inGroup'
                                                                )
                                                            ).length == 1,
                                                        'line-bottom-no-left-after': idxSub == findMaxIngroup(sub.promotionLineSubSub),
                                                        'line-straight': idxSub > 1,
                                                        'none-bottom': idxSub == sub.promotionLineSubSub.filter((e) => e.status != 'D').length
                                                    }">
                                                        <div class="flex flex-column gap-1 card mb-5">
                                                            <div class="grid m-0"
                                                                v-for="(subGroup, idx) in subSub"
                                                                :key="subGroup">
                                                                <div class="col-1 w-5rem"></div>
                                                                <div class="card line-w-sm line p-3 bg-gray-50 col bg-gray-200"
                                                                    :class="{
                                                                        'line-bottom': idx == 0 && subSub.length > 1,
                                                                        'line-straight': idx >= 1 && idx + 1 != subSub.length,
                                                                        'line-top ': idx + 1 == subSub.length && idx != 0
                                                                    }">
                                                                    <div class="flex justify-content-between">
                                                                        <div
                                                                            class="flex gap-3 align-items-center w-9">
                                                                            <span>{{
                                                                                t('body.promotion.quantity')
                                                                            }}</span>
                                                                            <InputNumber :min="0"
                                                                                v-model="subGroup.quantity"
                                                                                style="min-width: 70px;" />
                                                                            <InputGroup style="width: 300px"
                                                                                @click="emit('openSelectProduct', subGroup.promotionSubItemAdd)">
                                                                                <InputText
                                                                                    :placeholder="t('client.input_product')"
                                                                                    :invalid="subGroup.promotionSubItemAdd.length === 0"
                                                                                    :value="renderLabel(subGroup.promotionSubItemAdd)" />
                                                                                <InputGroupAddon>
                                                                                    <i
                                                                                        class="pi pi-list-check" />
                                                                                </InputGroupAddon>
                                                                            </InputGroup>
                                                                        </div>
                                                                        <div class="flex gap-2">
                                                                            <Button icon="pi pi-plus-circle"
                                                                                v-if="idx + 1 == subSub.length"
                                                                                @click="emit('addRows', sub.promotionLineSubSub, subGroup.inGroup, subGroup.cond, sub.isSameType)"
                                                                                text />
                                                                            <Button text icon="pi pi-clone"
                                                                                severity="info"
                                                                                @click="emit('CloneConditionSub', sub, subGroup)" />
                                                                            <Button
                                                                                @click="emit('RemoveItemProduct', sub, subGroup)"
                                                                                text icon="pi pi-trash"
                                                                                severity="danger" />
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div>
                                                    <Button :label="t('body.promotion.add_product_condition')"
                                                        icon="pi pi-plus" size="small" severity="secondary"
                                                        class="mb-3"
                                                        @click="emit('addRows', sub.promotionLineSubSub, sub.promotionLineSubSub?.filter((e) => e.status != 'D')?.length + 1, sub.isSameType)" />
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- SubType 2 -->
                                <div v-if="item.subType == 2">
                                    <div class="card p-4 border-round-md mb-5" v-for="subGroup in groupBy(
                                        item.promotionLineSub.filter((e) => e.status != 'D'),
                                        'followBy'
                                    )" :key="subGroup">
                                        <div class="border-round-md" v-for="(sub, subIndex) in subGroup"
                                            :key="sub">
                                            <div class="flex justify-content-between">
                                                <div class="flex flex-column gap-4 w-full">
                                                    <div class="flex gap-2 flex-column w-12 mt-2">
                                                        <div class="flex gap-2 align-items-center"
                                                            v-if="!subIndex">
                                                            <div class="w-4">
                                                                <Dropdown class="w-full" :options="optionFollowBy.filter((e) => {
                                                                    if (checkByIndex(item.promotionLineSub, item.subType)) {
                                                                        return e.code === sub.followBy;
                                                                    } else {
                                                                        return true;
                                                                    }
                                                                })
                                                                    " optionLabel="name" optionValue="code"
                                                                    @change="
                                                                        () => {
                                                                            emit('changeFormGroup', sub.followBy, subGroup);
                                                                            sub.includesP = false;
                                                                        }
                                                                    " v-model="sub.followBy" />
                                                            </div>
                                                            <div class="flex gap-2" v-if="sub.followBy == 2">
                                                                <Checkbox v-model="sub.includesP" binary />
                                                                <span>{{ t('body.promotion.includePromoVolume')
                                                                }}</span>
                                                            </div>
                                                        </div>
                                                        <div class="flex gap-3 w-12 mt-2"
                                                            v-if="!subIndex && sub.followBy == 2">
                                                            <div class="flex flex-column gap-2 w-6">
                                                                <h6 class="m-0 font-bold">
                                                                    {{ t('body.promotion.actualVolumeTime') }}
                                                                    <sup class="text-red-500">*</sup>
                                                                </h6>
                                                                <div class="flex gap-2">
                                                                    <Calendar
                                                                        :modelValue="sub.fromDate ? new Date(sub.fromDate) : ''"
                                                                        :invalid="submited && !sub.fromDate"
                                                                        v-model="sub.fromDate" class="w-5"
                                                                        placeholder="Từ" />
                                                                    <Calendar
                                                                        :modelValue="sub.toDate ? new Date(sub.toDate) : ''"
                                                                        :invalid="submited && !sub.toDate"
                                                                        v-model="sub.toDate" class="w-5"
                                                                        placeholder="Đến" />
                                                                </div>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div class="flex justify-content-between">
                                                        <div class="flex gap-2 align-items-center">
                                                            <span>Từ</span>
                                                            <InputNumber :min="0"
                                                                :placeholder="t('body.promotion.quantity_column')"
                                                                style="width: 70px" v-model="sub.quantity"
                                                                v-if="sub.followBy == 1" />

                                                            <div class="flex gap-2" v-if="sub.followBy == 2">
                                                                <InputGroup>
                                                                    <InputNumber :min="0"
                                                                        v-model="sub.minVolumn"
                                                                        :placeholder="t('body.promotion.volume')"
                                                                        inputStyle="width: 70px" />
                                                                    <InputGroupAddon>
                                                                        <span>Lít</span>
                                                                    </InputGroupAddon>
                                                                </InputGroup>
                                                            </div>

                                                            <InputGroup
                                                                @click="emit('openSelectProduct', sub.promotionItemBuy, true)"
                                                                style="width: 250px">
                                                                <InputText
                                                                    :placeholder="t('body.promotion.selectProductOrType')"
                                                                    :value="renderLabel(sub.promotionItemBuy)" />
                                                                <InputGroupAddon>
                                                                    <i class="pi pi-list-check" />
                                                                </InputGroupAddon>
                                                            </InputGroup>
                                                            <MultiSelect filter
                                                                v-if="sub.promotionItemBuy[0]?.itemType == 'G'"
                                                                optionLabel="packingName"
                                                                optionValue="packingId"
                                                                :options="setUnitProduct(sub.promotionItemBuy, sub)"
                                                                v-model="sub.promotionUnit"
                                                                selectedItemsLabel="Đã chọn {0} quy cách bao bì"
                                                                :maxSelectedLabels="2"
                                                                placeholder="Quy cách bao bì"
                                                                style="width: 250px" />
                                                            <div class="flex gap-2 align-items-center"
                                                                v-if="item.subType == 1">
                                                                <span>{{ t('body.promotion.get') }}</span>
                                                                <Dropdown :options="optionAddType"
                                                                    optionLabel="name" optionValue="code"
                                                                    v-model="sub.addType"
                                                                    style="width: 150px" />
                                                                <InputNumber :min="0" style="width: 100px"
                                                                    placeholder="Mua" v-if="sub.addType == 'R'"
                                                                    v-model="sub.addBuy" />
                                                                <InputNumber :min="0" v-model="sub.addQty"
                                                                    style="width: 100px"
                                                                    :placeholder="t('body.promotion.get')" />

                                                                <Checkbox v-model="sub.isSameType" binary
                                                                    @change="
                                                                        emit('changConditonTow',
                                                                            sub.promotionLineSubSub.filter((e) => e.status != 'D'),
                                                                            sub.isSameType
                                                                        )
                                                                        " />
                                                                <span>Khác loại</span>
                                                                <Dropdown v-if="sub.isSameType" class="w-4"
                                                                    :options="optionAddValueType"
                                                                    optionLabel="name" optionValue="code"
                                                                    v-model="sub.addValueType" />
                                                            </div>
                                                            <div class="flex align-items-center gap-2">
                                                                <span>{{
                                                                    t('body.OrderList.discount_column')
                                                                }}</span>
                                                                <InputNumber :min="0"
                                                                    :placeholder="t('body.promotion.buy')"
                                                                    v-if="sub.followBy" v-model="sub.discount"
                                                                    inputStyle="width: 100px" />
                                                                <div class="border-round flex justify-content-between"
                                                                    style="width: 100px">
                                                                    <div :class="{ 'bg-primary': sub.discountType == 'P' }"
                                                                        class="bg-gray-300 border-round-left w-6 text-center p-2 cursor-pointer"
                                                                        @click="(sub.discountType = 'P'), (sub.priceType = 'P')">
                                                                        <span>%</span>
                                                                    </div>
                                                                    <div :class="{
                                                                        'bg-primary': sub.discountType == 'C'
                                                                    }" class="bg-gray-300 border-round-right w-6 text-center p-2 cursor-pointer"
                                                                        @click="(sub.discountType = 'C'), (sub.priceType = 'C')">
                                                                        <i class="pi pi-dollar" />
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div class="flex gap-2">
                                                            <Button text icon="pi pi-plus-circle"
                                                                @click="emit('AddConditionQuantity', item, sub)"
                                                                v-if="subGroup.length == subIndex + 1" />
                                                            <Button text icon="pi pi-clone" severity="info"
                                                                @click="emit('CloneCondition', item, sub)" />
                                                            <Button text icon="pi pi-trash" severity="danger"
                                                                @click="emit('removeConditionGroup', item, sub)" />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <!-- Button thêm -->
                <div class="grid m-0 mt-3"
                    v-if="payload.promotionLine?.filter((l) => l.status != 'D' && !l.HasExceptionBlock).length < 2">
                    <div class="col-1 p-0"></div>
                    <div class="col p-0">
                        <div class="line line-top card w-15rem p-3">
                            <Button @click="emit('addForm')" severity="secondary"
                                class="w-full bg-gray-200 border-none hover:bg-green-200" icon="pi pi-plus"
                                :label="t('body.promotion.add_form')" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
