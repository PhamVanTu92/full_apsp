<template>
    <div>
        <div class="flex mb-3">
            <h4 class="font-bold m-0">{{ t('body.SaleSchannel.interface_management_title') }}</h4>
        </div>
    </div>
    <div class="card p-3">
        <div class="flex justify-content-end mb-3">
            <Button :label="t('body.SaleSchannel.save_button')"/>
        </div>
        <!-- <hr class="mb-0" /> -->
        <Accordion :activeIndex="0" class="border-1 border-200">
            <AccordionTab :header="t('body.sampleRequest.customerGroup.homepage_label')">
                <Home />
            </AccordionTab>
        </Accordion>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";

import Home from "./components/Home/Home.vue";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

onMounted(function () {});
</script>

<style scoped></style>
