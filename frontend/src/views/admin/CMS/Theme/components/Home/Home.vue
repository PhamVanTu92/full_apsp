<template>
    <div>
        <div class="grid">
            <div class="col-6">
                <div class="relative">
                    <div
                        ref="frame"
                        class="absolute top-0 left-0 right-0 bottom-0 hidden"
                    >
                        <div ref="blur_top" class="surface-200 bg-black-alpha-50"></div>
                        <div ref="focus_frame" class="border-3 border-red-500">
                            <div class="flex justify-content-end">
                                <div class="p-1 bg-red-500 text-white">{{ label }}</div>
                            </div>
                        </div>
                        <div
                            ref="blur_bottom"
                            class="surface-200 bg-black-alpha-30"
                        ></div>
                    </div>
                    <img
                        id="image"
                        src="../../../../../../../public/image/CMS/Theme/client.png"
                        alt=""
                        class="w-full border-1 border-200"
                    />
                </div>
            </div>
            <div class="col-6">
                <!-- :activeIndex="0" -->

                <Accordion
                    @tab-close="onClose"
                    @tab-open="onOpen"
                    class="border-1 border-200"
                >
                    <AccordionTab :header="t('body.SaleSchannel.product_group_1_label')">
                        <ProductGroup />
                    </AccordionTab>
                    <AccordionTab :header="t('body.SaleSchannel.product_group_2_label')">
                        <ProductGroup1 />
                    </AccordionTab>
                    <AccordionTab header="Banner">
                        <Banner />
                    </AccordionTab>
                    <AccordionTab header="Footer">
                        <Footer />
                    </AccordionTab>
                </Accordion>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted } from "vue";
import ProductGroup from "./AccordionTabContent/ProductGroup.vue";
import ProductGroup1 from "./AccordionTabContent/ProductGroup1.vue";
import Banner from "./AccordionTabContent/Banner.vue";
import Footer from "./AccordionTabContent/Footer.vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const onOpen = (e: any) => {

    frame.value?.classList.remove("hidden");
    toggleFocus(e.index);
};
const onClose = (e: any) => {

    frame.value?.classList.toggle("hidden");
    toggleFocus(e.index);
};
const label = ref();
const frame = ref<HTMLElement>();
const blur_top = ref<HTMLElement>();
const focus_frame = ref<HTMLElement>();
const blur_bottom = ref<HTMLElement>();
const toggleFocus = (index: number) => {
    const focusIndex = axisRangers.value.findIndex((el) => el.index == index);
    const blur_top_height = axisRangers.value
        .filter((el, i) => i < focusIndex)
        ?.reduce((a, b) => {
            return a + b.height;
        }, 0);
    const focus_frame_height = axisRangers.value[focusIndex].height;
    label.value = axisRangers.value[focusIndex].name;
    const blur_bottom_height = 100 - (blur_top_height + focus_frame_height);
    if (blur_top.value) blur_top.value.style.height = `${blur_top_height}%`;
    if (focus_frame.value) focus_frame.value.style.height = `${focus_frame_height}%`;
    if (blur_bottom.value) blur_bottom.value.style.height = `${blur_bottom_height}%`;
};

const axisRangers = ref([
    {
        id: 0,
        index: null,
        name: "",
        height: 4.7,
    },
    {
        id: 1,
        index: null,
        name: "",
        height: 2,
    },
    {
        id: 2,
        index: 0,
        name: t('body.SaleSchannel.product_group_1_label'),
        height: 31,
    },
    {
        id: 3,
        index: 1,
        name: t('body.SaleSchannel.product_group_2_label'),
        height: 34,
    },
    {
        id: 4,
        index: 2,
        name: "Banner",
        height: 12.5,
    },
    {
        id: 5,
        index: 3,
        name: "Footer",
        height: 15.72,
    },
]);

onMounted(function () {});
</script>

<style scoped></style>
