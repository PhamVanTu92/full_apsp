<template>
    <div>
        <div class="mb-3 justify-content-between">
            <h4 class="font-bold mb-0">{{ t('body.SaleSchannel.menu_management_title') }}</h4>
        </div>
        <div class="card p-3">
            <div class="flex align-items-center gap-2 mb-3">
                <span>{{ t('body.SaleSchannel.select_menu_to_edit_label') }}:</span>
                <Dropdown class="w-20rem" :placeholder="t('body.SaleSchannel.select_menu_placeholder')">
                    <template #empty>
                        <div class="text-500 font-italic my-5 py-5 text-center">
                            {{ t('body.systemSetting.no_data_to_display') }}
                        </div>
                    </template>
                </Dropdown>
                <Button :label="t('body.SaleSchannel.add_new_button')" icon="pi pi-plus"/>
                <span>{{ t('body.SaleSchannel.add_new_menu_instruction') }}</span>
            </div>
            <hr />
            <div>
                <h5 class="font-bold">{{ t('body.SaleSchannel.menu_structure_title') }}</h5>
                <div class="border-1 border-200">
                    <div class="p-3 surface-200 flex justify-content-between">
                        <div>
                            <span class="mr-2">{{ t('body.SaleSchannel.menu_name_label') }}:</span>
                            <InputText :placeholder="t('body.SaleSchannel.enter_menu_name_placeholder')"></InputText>
                        </div>
                        <Button :label="t('body.SaleSchannel.save_button')"/>
                    </div>
                    <div class="p-3">
                        <div>
                            {{ t('body.SaleSchannel.drag_and_drop_instruction') }}
                        </div>
                        <hr />
                        <div class="">
                            <div class="">
                                <DraggableTree :items="myArray">
                                    <template #body="sp">
                                        <template>
                                            {{ (sp.data.title_new = sp.data.title) }}
                                        </template>
                                        <div class="border-1 border-200 mb-2 w-30rem">
                                            <div class="p-2 cursor-move justify-content-between flex">
                                                <span class="my-auto">{{
                                                    sp.data.title
                                                }}</span>

                                                <span>
                                                    <Button :icon="`pi pi-angle-${sp.data.toggle ? 'up' : 'down'
                                                        }`" class="p-0 w-2rem h-2rem" text
                                                        @click="sp.data.toggle = !sp.data.toggle"
                                                        severity="secondary"/>
                                                </span>
                                            </div>
                                            <div class="p-3" v-if="sp.data.toggle">
                                                <div class="mb-3">
                                                    <div class="flex flex-column gap-1 mb-3">
                                                        <label for="">{{ t('body.SaleSchannel.label') }}:</label>
                                                        <InputText v-model="sp.data.title_new"></InputText>
                                                    </div>
                                                    <div class="flex flex-column gap-1">
                                                        <label for="">{{ t('body.SaleSchannel.link') }}:</label>
                                                        <InputText v-model="sp.data.uri"></InputText>
                                                    </div>
                                                </div>
                                                <div class="flex justify-content-between">
                                                    <Button :label="t('body.SaleSchannel.add_child_menu')"/>
                                                    <Button :label="t('body.OrderList.delete')"
                                                        severity="danger"/>
                                                </div>
                                            </div>
                                        </div>
                                    </template>
                                </DraggableTree>
                                <Button :label="t('body.sampleRequest.customerGroup.add_menu_button')"/>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, reactive, computed, watch, onMounted } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const initialComponent = () => {
    // code here
};

const myArray = ref([
    {
        id: 1,
        title: "item 1",
        children: [
            {
                id: 4,
                title: "item 4",
                children: [],
            },
            {
                id: 5,
                title: "item 5",
                children: [],
            },
        ],
    },
    {
        id: 2,
        title: "item 2",
        children: [],
    },
    {
        id: 3,
        title: "item 3",
        children: [],
    },
]); 

const dragOptions = {
    animation: 0,
    group: "description",
    disabled: false,
    ghostClass: "ghost",
};

onMounted(function () {
    initialComponent();
});
</script>

<style scoped></style>
