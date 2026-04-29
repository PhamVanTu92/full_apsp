<script setup>
import { groupBy } from 'lodash';
import { inject } from 'vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const props = defineProps({
    payload: { type: Object, required: true },
    submited: { type: Boolean, default: false },
    optionFollowBy: { type: Array, required: true },
    optionFollowBy2: { type: Array, required: true },
    optionAddType: { type: Array, required: true },
    optionAddValueType: { type: Array, required: true },
});

const emit = defineEmits([
    'changeHasEx', 'addCondition', 'AddFormHasException',
    'removeForm', 'removeCondition', 'removeConditionGroup',
    'openSelectProduct', 'CloneCondition',
    'AddConditionQuantity', 'ChangeSubFrom',
    'changeFormGroup', 'changConditonTow',
]);

const checkSubTypeHasException = inject('checkSubTypeHasException');
const checkByIndex = inject('checkByIndex');
const getFirstIndex = inject('getFirstIndex');
const renderLabel = inject('renderLabel');
const setUnitProduct = inject('setUnitProduct');
</script>
<template>
    <!-- Điều kiện ngoại lệ -->
    <div class="grid mt-3">
        <div class="col-1"></div>
        <div class="col-11">
            <div class="flex gap-2">
                <Checkbox v-model="payload.hasException" @change="emit('changeHasEx', payload.hasException)"
                    binary />
                <label for="">{{ t('body.promotion.exception') }}</label>
            </div>
            <div class="card mt-3" v-if="payload.hasException">
                <h6 class="m-0 font-bold">{{
                    t('body.promotion.simultaneous_promotions_section_title') }}</h6>
                <div class="ml-2 mt-3">
                    <div class="flex flex-column gap-3">
                        <div class="flex gap-2">
                            <Checkbox v-model="payload.isOtherPromotionExc" binary />
                            <label for="">{{ t('body.promotion.simultaneous_promotion_checkbox')
                            }}</label>
                        </div>
                        <div class="flex gap-2">
                            <Checkbox v-model="payload.isOtherPayExc" binary />
                            <label for="">{{ t('body.promotion.simultaneous_payment_checkbox')
                            }}</label>
                        </div>
                        <div class="flex gap-2">
                            <Checkbox v-model="payload.isOtherDistExc" binary />
                            <label for="">{{ t('body.promotion.simultaneous_discount_checkbox')
                            }}</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="grid"
        v-for="(item, index) in payload.promotionLine.filter((e) => e.status != 'D' && e.HasExceptionBlock)"
        :key="item">
        <div class="col-1"></div>
        <div class="col-11">
            <div class="card mt-3 bg-gray-200" v-if="payload.hasException">
                <div class="card mt-3">
                    <div class="flex justify-content-between">
                        <div class="flex gap-3 align-items-center w-6">
                            <h6 class="m-0 font-bold">Hình thức</h6>
                            <Dropdown class="w-4"
                                :options="optionFollowBy2.filter((e) => checkSubTypeHasException(item.subType).includes(e.code))"
                                optionLabel="name" optionValue="code"
                                @change="emit('ChangeSubFrom', item.promotionLineSub, item.subType)"
                                v-model="item.subType" />
                        </div>
                        <div class="w-6 text-right">
                            <Button v-if="index" :label="t('body.promotion.deleteOfferType')"
                                outlined size="small" severity="danger"
                                @click="emit('removeForm', payload, item)" />
                        </div>
                    </div>
                    <hr />
                    <div class="mb-2 flex justify-content-end">
                        <Button :label="t('body.promotion.addPromotionCondition')" icon="pi pi-plus"
                            size="small" severity="secondary"
                            @click="emit('addCondition', item.promotionLineSub, item.subType)"
                            v-if="!checkByIndex(item.promotionLineSub) || item.subType == 1" />
                    </div>
                    <p class="m-0 font-bold mb-3">{{ t('body.promotion.promotion') }}</p>
                    <!-- SubType 1 ngoại lệ -->
                    <div v-if="item.subType == 1">
                        <div class="flex flex-column p-4"
                            v-for="(sub, subIndex) in item.promotionLineSub.filter((e) => e.status != 'D')"
                            :key="sub">
                            <!-- begin line promotion -->
                            <div class="flex gap-2 flex-column w-full mt-2"
                                v-if="item.subType == 2">
                                <div class="flex gap-2 align-items-center mb-3"
                                    v-if="getFirstIndex(item.promotionLineSub) == subIndex || sub.followBy == 2">
                                    <div class="w-3">
                                        <Dropdown class="w-full" :options="optionFollowBy2.filter((e) => {
                                            if (checkByIndex(item.promotionLineSub, item.subType)) {
                                                return e.code === sub.followBy;
                                            } else {
                                                return true;
                                            }
                                        })
                                            " optionLabel="name" optionValue="code"
                                            v-model="sub.followBy" />
                                    </div>
                                    <div class="flex gap-2">
                                        <Checkbox />
                                        <span>{{ t('body.promotion.includePromoVolume') }}</span>
                                    </div>
                                </div>
                                <div class="flex gap-3 w-12 mb-5" v-if="sub.followBy == 2">
                                    <div class="flex flex-column gap-2 w-6">
                                        <h6 class="m-0 font-bold">
                                            {{ t('body.promotion.actualVolumeTime') }}
                                        </h6>
                                        <div class="flex gap-2">
                                            <Calendar
                                                :modelValue="sub.fromDate ? new Date(sub.fromDate) : ''"
                                                v-model="sub.fromDate" class="w-5"
                                                :placeholder="t('body.promotion.from_label')" />
                                            <Calendar
                                                :modelValue="sub.toDate ? new Date(sub.toDate) : ''"
                                                v-model="sub.toDate" class="w-5"
                                                placeholder="Đến" />
                                        </div>
                                    </div>
                                    <div class="flex flex-column gap-2 w-6">
                                        <h6 class="m-0 font-bold">Sản lượng tối thiểu</h6>
                                        <div class="flex gap-2">
                                            <InputGroup class="w-4">
                                                <InputNumber :min="0" v-model="sub.minVolumn"
                                                    placeholder="Sản lượng"
                                                    style="min-width: 70px;" />
                                                <InputGroupAddon>
                                                    <span>Lít</span>
                                                </InputGroupAddon>
                                            </InputGroup>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="flex align-items-center justify-content-between">
                                <div class="flex gap-2 align-items-center w-11">
                                    <span>Nhỏ hơn</span>
                                    <InputNumber :min="0" placeholder="Số lượng"
                                        v-model="sub.quantity" style="min-width: 70px;" />
                                    <InputGroup style="width: 300px"
                                        @click="emit('openSelectProduct', sub.promotionItemBuy, true)">
                                        <InputText placeholder="Chọn sản phẩm/loại sản phẩm"
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
                                        :maxSelectedLabels="2" placeholder="Quy cách bao bì" />
                                    <div class="flex gap-2 align-items-center"
                                        v-if="item.subType == 1">
                                        <span>{{ t('body.promotion.get') }}</span>
                                        <Dropdown :options="optionAddType" optionLabel="name"
                                            optionValue="code" v-model="sub.addType"
                                            style="width: 150px" />
                                        <InputNumber :min="0" style="width: 100px" placeholder="Mua"
                                            v-if="sub.addType == 'R'" v-model="sub.addBuy" />
                                        <InputNumber :min="0" v-model="sub.addQty"
                                            style="width: 100px"
                                            :placeholder="t('body.promotion.get')" />
                                        <Checkbox v-model="sub.isSameType" binary />
                                        <span>Khác loại</span>
                                        <Dropdown v-if="sub.isSameType" class="w-4"
                                            :options="optionAddValueType" optionLabel="name"
                                            optionValue="code" v-model="sub.addValueType" />
                                    </div>
                                    <!-- Giảm giá hàng -->
                                    <div class="flex align-items-center gap-2"
                                        v-if="item.subType == 2">
                                        <div>
                                            <Checkbox v-model="sub.isSameType" binary
                                                class="mr-2" />
                                            <label for="">Khác loại</label>
                                        </div>
                                        <span class="ml-4">{{ t('body.OrderList.discount_column')
                                        }}</span>
                                        <InputNumber :min="0" style="width: 100px"
                                            :placeholder="t('body.promotion.buy')"
                                            v-if="sub.followBy" v-model="sub.discount" />
                                        <div class="border-round flex justify-content-between"
                                            style="width: 100px">
                                            <div :class="{
                                                'bg-primary': sub.discountType == 'P'
                                            }" class="bg-gray-300 border-round-left w-6 text-center p-2 cursor-pointer"
                                                @click="sub.discountType = 'P'">
                                                <span>%</span>
                                            </div>
                                            <div :class="{
                                                'bg-primary': sub.discountType == 'M'
                                            }" class="bg-gray-300 border-round-right w-6 text-center p-2 cursor-pointer"
                                                @click="sub.discountType = 'M'">
                                                <span>VNĐ</span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <div class="flex gap-2">
                                    <Button text icon="pi pi-plus-circle"
                                        v-if="item.promotionLineSub.filter((e) => !e.followBy).length == subIndex + 1"
                                        @click="emit('AddConditionQuantity', item, sub)" />
                                    <Button text icon="pi pi-clone" severity="info"
                                        @click="emit('CloneCondition', item, sub)" />
                                    <Button icon="pi pi-trash" text severity="danger"
                                        @click="emit('removeCondition', item, sub, subIndex)" />
                                </div>
                            </div>
                            <hr />
                            <!-- end -->
                        </div>
                    </div>
                    <!-- SubType 2 ngoại lệ -->
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
                                                    <span>{{ t('body.promtion.includePromoVolume')
                                                    }}</span>
                                                </div>
                                            </div>
                                            <div class="flex gap-3 w-12 mt-2"
                                                v-if="!subIndex && sub.followBy == 2">
                                                <div class="flex flex-column gap-2 w-6">
                                                    <p class="m-0 font-bold mb-2">
                                                        {{ t('body.promotion.actualVolumeTime') }}
                                                        <sup class="text-red-500">*</sup>
                                                    </p>
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
                                            <div class="flex gap-2 align-items-center w-10">
                                                <span class="w-5rem">Nhỏ hơn</span>
                                                <InputNumber :min="0"
                                                    :placeholder="t('body.home.quantity')"
                                                    v-model="sub.quantity" v-if="sub.followBy == 1"
                                                    style="min-width: 70px;" />

                                                <div class="flex gap-2" v-if="sub.followBy == 2">
                                                    <InputGroup>
                                                        <InputNumber :min="0"
                                                            v-model="sub.minVolumn"
                                                            :placeholder="t('body.promotion.volume')"
                                                            style="min-width: 70px;" />
                                                        <InputGroupAddon>
                                                            <span>Lít</span>
                                                        </InputGroupAddon>
                                                    </InputGroup>
                                                </div>

                                                <InputGroup style="width: 300px"
                                                    @click="emit('openSelectProduct', sub.promotionItemBuy, true)">
                                                    <InputText
                                                        :placeholder="t('body.promotion.selectProductOrType')"
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
                                                    :maxSelectedLabels="2"
                                                    :placeholder="t('body.promotion.packaging_column')" />
                                                <div class="flex gap-2 align-items-center"
                                                    v-if="item.subType == 1">
                                                    <span>{{ t('body.promotion.get') }}</span>
                                                    <Dropdown :options="optionAddType"
                                                        optionLabel="name" optionValue="code"
                                                        v-model="sub.addType"
                                                        style="width: 150px" />
                                                    <InputNumber :min="0" style="width: 100px"
                                                        :placeholder="t('body.promotion.buy')"
                                                        v-if="sub.addType == 'R'"
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
                                                    <span>{{ t('body.promotion.different_type')
                                                    }}</span>
                                                    <Dropdown v-if="sub.isSameType" class="w-4"
                                                        :options="optionAddValueType"
                                                        optionLabel="name" optionValue="code"
                                                        v-model="sub.addValueType" />
                                                </div>
                                                <div
                                                    class="flex-grow-0 flex align-items-center gap-2">
                                                    <span class="ml-4">{{
                                                        t('body.OrderList.discount_column')
                                                    }}</span>
                                                    <InputNumber :min="0" style="width: 100px"
                                                        placeholder="Mua" v-if="sub.followBy"
                                                        v-model="sub.discount" />
                                                    <div class="border-round flex justify-content-between"
                                                        style="width: 100px">
                                                        <div :class="{
                                                            'bg-primary': sub.discountType == 'P'
                                                        }" class="bg-gray-300 border-round-left w-6 text-center p-2 cursor-pointer"
                                                            @click="sub.discountType = 'P'">
                                                            <span>%</span>
                                                        </div>
                                                        <div :class="{
                                                            'bg-primary': sub.discountType == 'M'
                                                        }" class="bg-gray-300 border-round-right w-6 text-center p-2 cursor-pointer"
                                                            @click="sub.discountType = 'M'">
                                                            <span>VNĐ</span>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="flex gap-2">
                                                <Button text icon="pi pi-plus-circle"
                                                    v-if="subGroup.length == subIndex + 1"
                                                    @click="emit('AddConditionQuantity', item, sub)" />
                                                <Button text icon="pi pi-clone" severity="info"
                                                    @click="emit('CloneCondition', item, sub)" />
                                                <Button text icon="pi pi-trash" severity="danger"
                                                    @click="emit('removeConditionGroup', item, sub)" />
                                            </div>
                                        </div>
                                        <hr />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div
                    v-if="payload.promotionLine?.filter((l) => l.status != 'D' && l.HasExceptionBlock).length < 2">
                    <Button icon="pi pi-plus-circle" label="Thêm hình thức ngoại lệ" size="small"
                        @click="emit('AddFormHasException', payload.promotionLine)" severity="secondary" />
                </div>
            </div>
        </div>
    </div>
</template>
