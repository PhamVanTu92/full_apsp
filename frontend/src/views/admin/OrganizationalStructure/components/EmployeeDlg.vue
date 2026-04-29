<template>
    <Dialog
        v-model:visible="visible"
        dataKey="id"
        :header="t('body.systemSetting.employee_management')"
        modal
        class="w-5"
        @hide="onHide"
    >
        <PickList
            id="picklistUser"
            v-model="employees"
            listStyle="height:50vh;"
            breakpoint="1400px"
            :showSourceControls="false"
            :showTargetControls="false"
        >
            <template #sourceheader>
                <InputText
                    @input="onSearch"
                    v-model="query.search"
                    :placeholder="t('body.home.search_placeholder')"
                    class="w-full"
                />
            </template>
            <template #targetheader>
                <div class="py-2">{{t('body.systemSetting.employee_selected')}} {{ employees[1].length }}</div>
            </template>
            <template #item="slotProps">
                <div class="flex gap-3">
                    <div
                        class="flex justify-content-center align-items-center w-3rem h-3rem border-circle border-1 border-200 surface-300"
                    >
                        <i class="pi pi-user text-500 text-lg"></i>
                    </div>
                    <div>
                        <div class="flex">
                            <div class="font-bold">{{ slotProps.item.fullName }}</div>
                        </div>
                        <div
                            class="text-500 text-md overflow-hidden text-overflow-ellipsis w-10rem"
                        >
                            {{ slotProps.item.email }}
                        </div>
                    </div>
                </div>
            </template>
            <template #empty>ds</template>
        </PickList>
        <!-- {{ selection }} -->
        <template #footer>
            <Button
                @click="visible = false"
                icon="pi pi-times"
                :label="t('body.OrderList.close')"
                severity="secondary"
            />
            <Button @click="onClickSave" icon="pi pi-save" :label="t('body.promotion.save_button')" />
        </template>
    </Dialog>
</template>

<script setup lang="ts">
import { ref, reactive, computed, onMounted, watch, nextTick } from "vue";
import API from "@/api/api-main";
import debounce from "lodash/debounce";
import {useInfiniteScroll} from '@vueuse/core'
import { Employee } from "../script";
import { useI18n } from 'vue-i18n';

const { t } = useI18n();


const emits = defineEmits(["save"]);
const selection = defineModel("selection", {
    default: [],
    type: Array<Employee>,
});

const employees = ref([[] as Array<Employee>, [] as Array<Employee>]);

const visible = ref(false);

const onSearch = debounce(()=> {
    employees.value[0] = [];
    query.skip = 0;
    fetchUser(query.toQueryString());
},200)

const onClickSave = () => {
    const data = employees.value[1].map((epl) => epl.id);
    emits("save", data);
    onHide();
};

const query = reactive({
    skip: 0,
    limit: 30,
    search: "",
    Filter: '(organizationId=null,status=A)',
    userType: 'APSP',
    toQueryString: () => {
        let result:Array<string> = [];
        Object.keys(query).forEach(key => {
            if(key != 'toQueryString')
            result.push(`${key}=${query[key]}`);
        })
        return result.join('&');
    }
})
const loading = ref(false);
const error = ref(false);
const fetchUser = (_queryString: string) => {
    loading.value = true;
    API.get("account/getall?"+_queryString).then((res) => {
        const data = res.data.item as Array<Employee>;
        if(data.length < 1) error.value = true;
        // const _data = data.filter(e => !employees.value[1].map(el => el.id).includes(e.id))
        employees.value[0] = [...employees.value[0], ...data];
    }).catch(err => {
        error.value = true;
    })
    .finally(() => {
        loading.value = false;
    });
};

const onHide = () => {
    query.skip = 0;
    query.search = '';
    visible.value = false;
}

const open = () => {
    visible.value = true;
    employees.value[0] = [];
    employees.value[1] = [];
    query.skip = 0;
    error.value = false;
    loading.value = false;
    nextTick(() => {
        picklistUser.value = document.querySelector('#picklistUser .p-picklist-source-list')
    })
    if(selection.value.length > 0) {
        employees.value[1] = [];
        // selection.value
    }
};

const picklistUser = ref<any>();
useInfiniteScroll(picklistUser,()=> {
    // fetch data
    if(error.value || loading.value) return;
    fetchUser(query.toQueryString());
    query.skip++;
}, {
    distance: 10
})

defineExpose({
    open,
});

onMounted(function () {
});
</script>

<style scoped></style>
