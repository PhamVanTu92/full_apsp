<script setup>
import format from '@/helpers/format.helper';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();

const props = defineProps({
    payload: { type: Object, required: true }
});
</script>
<template>
    <div class="card">
        <h6 class="m-0 font-bold mb-3">{{ t('body.promotion.summary_section_title') }}</h6>
        <div class="flex gap-2 ml-2">
            <p>{{ t('body.promotion.summary_promotion_name') }}</p>
            <p>{{ payload.promotionName }}</p>
        </div>
        <div class="flex gap-2 ml-2">
            <p>{{ t('body.promotion.summary_status') }}</p>
            <p>
                {{ payload.promotionStatus == 'A' ? t('body.promotion.summary_status_value') :
                    t('body.promotion.status_inactive') }}
            </p>
        </div>
        <div class="flex gap-2 ml-2">
            <p>{{ t('body.promotion.summary_applicable_object') }}</p>
            <p>
                {{ payload.isAllCustomer ? t('body.promotion.summary_applicable_object_value') :
                    payload.promotionCustomer.type == 'C' ? t('body.report.customer_label_1') :
                        t('body.sampleRequest.customerGroup.title') }}
            </p>
        </div>
        <div class="flex gap-2 ml-2">
            <p>{{ t('body.promotion.applicable_time_section_title') }}</p>
            <p>
                {{ payload.fromDate ? format.formatDate(payload.fromDate, 'dd/MM/yyyy') : '' }}
                -
                {{ payload.toDate ? format.formatDate(payload.toDate, 'dd/MM/yyyy') : '' }}
            </p>
        </div>
    </div>
</template>
