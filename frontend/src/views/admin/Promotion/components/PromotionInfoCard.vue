<script setup>
import SelectDistributor from './SelectDistributor.vue';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const props = defineProps({
    payload: { type: Object, required: true },
    submited: { type: Boolean, default: false },
    loading: { type: Boolean, default: false }
});
</script>
<template>
    <div class="card">
        <h6 class="m-0 font-bold">{{ t('body.promotion.info_section_title') }}</h6>
        <div class="grid mt-2">
            <div class="col-12 flex flex-column gap-3">
                <div class="flex justify-content-between">
                    <span>{{ t('body.promotion.promotion_code_label') }}</span>
                    <InputText v-model="payload.promotionCode" disabled class="w-8" :maxlength="50"
                        :placeholder="t('body.promotion.promotion_code_label_max')" />
                </div>
                <div class="flex justify-content-between">
                    <span>{{ t('body.promotion.promotion_name_label') }} <sup
                            class="text-red-500">*</sup></span>
                    <IconField class="w-8">
                        <InputText v-model="payload.promotionName" placeholder="Tên khuyến mãi" />
                        <!-- <InputIcon class="pi pi-sparkles cursor-pointer text-purple-500" @click="generateAI"
                            v-tooltip.top="t('body.promotion.generate_ai_tooltip')" /> -->
                    </IconField>
                    <!-- <InputText :invalid="submited && !payload.promotionName" v-model="payload.promotionName" class="w-8" /> -->
                </div>
            </div>
            <div class="col-12 flex justify-content-between">
                <span>{{ t('body.promotion.notes_label') }}</span>
                <Textarea v-model="payload.note" rows="3" class="w-8"></Textarea>
            </div>
            <div class="col-12 flex flex-column gap-3">
                <div class="flex justify-content-between">
                    <span>{{ t('body.promotion.status_column') }}</span>
                    <div class="flex w-8 gap-6">
                        <div class="flex align-items-center">
                            <RadioButton inputId="A" v-model="payload.promotionStatus" value="A" />
                            <label for="A" class="ml-2">{{ t('body.promotion.status_active') }}</label>
                        </div>
                        <div class="flex align-items-center">
                            <RadioButton inputId="I" v-model="payload.promotionStatus" value="I" />
                            <label for="I" class="ml-2">{{ t('body.promotion.status_inactive') }}</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="grid mt-2">
            <div class="col-12 flex flex-column gap-3">
                <h6 class="m-0 font-bold">{{ t('body.promotion.applicable_object_section_title') }}</h6>
                <div class="flex justify-content-between">
                    <div class="flex align-items-center">
                        <RadioButton v-model="payload.isAllCustomer" :value="true" />
                        <label class="ml-2">{{ t('body.promotion.applicable_object_all') }}</label>
                    </div>
                </div>
                <div class="flex">
                    <div class="flex w-4 align-items-center">
                        <RadioButton v-model="payload.isAllCustomer" :value="false" />
                        <label class="ml-2">{{ t('body.promotion.applicable_object_custom') }}</label>
                    </div>
                    <div class="flex w-8 flex-column gap-2">
                        <SelectDistributor v-if="!loading" v-model:selection="payload.promotionCustomer"
                            :disabled="payload.isAllCustomer" />
                        <div class="flex align-items-center ml-3">
                            <Checkbox v-model="payload.isIgnore" binary :disabled="payload.isAllCustomer" />
                            <label class="ml-2">{{ t('body.promotion.applicable_object_remove') }}</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="col-12 flex flex-column gap-3">
                <h6 class="m-0 font-bold">{{ t('body.promotion.applicable_time_section_title') }}</h6>
                <div class="flex justify-content-between">
                    <span>{{ t('body.promotion.start_date_label') }}<sup class="text-red-500">*</sup></span>
                    <Calendar class="w-8" showIcon :invalid="submited && !payload.fromDate"
                        v-model="payload.fromDate" :placeholder="t('body.promotion.start_date_placeholder')"
                        :maxDate="payload.toDate" />
                </div>
                <div class="flex justify-content-between">
                    <span>{{ t('body.promotion.end_date_label') }}<sup class="text-red-500">*</sup></span>
                    <Calendar class="w-8" showIcon :invalid="submited && !payload.toDate"
                        v-model="payload.toDate" :placeholder="t('body.promotion.end_date_placeholder')"
                        :minDate="payload.fromDate" />
                </div>
            </div>
            <div class="col-12">
                <div class="flex align-items-center">
                    <Checkbox v-model="payload.isBirthday" binary />
                    <label class="ml-2">
                        {{ t('body.promotion.birthday_promotion_checkbox') }}
                    </label>
                </div>
                <div class="col-12 pb-0 ml-3" v-if="payload.isBirthday">
                    <span class="font-italic font-bold">{{ t('Custom.effectiveTime') }}</span>
                    <div class="flex gap-3 mt-2">
                        <div class="flex flex-column field mb-0">
                            <label class="font-italic" for="before-bd">{{ t('Custom.beforeBirthday') }}</label>
                            <InputGroup class="w-11rem">
                                <InputNumber inputId="before-bd" v-model="payload.beforeDay" :min="0"
                                    :max="30" />
                                <InputGroupAddon>{{ t('Custom.days') }}</InputGroupAddon>
                            </InputGroup>
                        </div>
                        <div class="flex flex-column field mb-0">
                            <label class="font-italic" for="affter-bd">{{ t('Custom.afterBirthday') }}</label>
                            <InputGroup class="w-11rem">
                                <InputNumber inputId="affter-bd" :min="0" :max="30"
                                    v-model="payload.afterDay" />
                                <InputGroupAddon>{{ t('Custom.days') }}</InputGroupAddon>
                            </InputGroup>
                        </div>
                        <div class="flex flex-column field mb-0 ml-8">
                            <label class="font-italic">{{ t('Custom.promotionMethod') }}</label>
                            <div class="flex gap-3 mt-2">
                                <div class="flex gap-2">
                                    <Checkbox inputId="paynow" v-model="payload.isPayNow" binary />
                                    <label for="paynow">{{ t('Custom.paymentNow') }}</label>
                                </div>
                                <div class="flex gap-2">
                                    <Checkbox inputId="cnbl" v-model="payload.isCreditGuarantee" binary />
                                    <label for="cnbl">{{ t('Custom.debtGuaranteed') }}</label>
                                </div>
                                <div class="flex gap-2">
                                    <Checkbox inputId="cntc" v-model="payload.isCredit" binary />
                                    <label for="cntc">{{ t('Custom.debtUnsecured') }}</label>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <hr />
                <div class="flex flex-column gap-3">
                    <h6 class="m-0 font-bold">{{ t('body.promotion.simultaneous_promotions_section_title') }}
                    </h6>
                    <div class="flex align-items-center">
                        <Checkbox v-model="payload.isOtherPromotion" binary />
                        <label class="ml-2"> {{ t('body.promotion.simultaneous_promotion_checkbox') }} </label>
                    </div>
                    <div class="flex align-items-center">
                        <Checkbox v-model="payload.isOtherPay" binary />
                        <label class="ml-2"> {{ t('body.promotion.simultaneous_payment_checkbox') }}</label>
                    </div>
                    <div class="flex align-items-center">
                        <Checkbox v-model="payload.isOtherDist" binary />
                        <label class="ml-2">
                            {{ t('body.promotion.simultaneous_discount_checkbox') }}
                        </label>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
