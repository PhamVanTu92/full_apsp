<template>
    <div ref="target" class="w-full relative">
        <InputText 
            v-model="key" 
            :placeholder="placeholder" 
            class="w-full z-0" 
            @focus="onFocusInput"
            @click="() => {visible = true}">
        </InputText>
        <div class="tab-view w-full border-1 border-300 absolute z-1" v-if="visible">
            <TabView v-model:activeIndex="activeIndex" class="w-full" :pt="ptTabview">
                <TabPanel header="Tỉnh/Thành phố">
                    <Listbox @change="onChangeCity" v-model="address.city" :options="cities" optionLabel="name" class="w-full border-0" listStyle="max-height:250px" />
                </TabPanel>
                <TabPanel header="Quận/Huyện">
                    <Listbox @change="onChangeDistrict" v-model="address.district" :options="districts" optionLabel="name" class="w-full border-0" listStyle="max-height:250px" />
                </TabPanel>
                <TabPanel header="Phường/Xã">
                    <Listbox @change="onChangeWard" v-model="address.ward" :options="wards" optionLabel="name" class="w-full border-0" listStyle="max-height:250px" />
                </TabPanel>
            </TabView>
        </div>
    </div>
    

</template>

<script setup>
import { onClickOutside } from '@vueuse/core';
import { ref, reactive, onMounted } from 'vue'

import citiesData from './data/location.json';

const model = defineModel()

onMounted(() => {
    if(model.value){
        key.value = `${model.value.city}, ${model.value.district}, ${model.value.ward}`;
        address.city = model.value.city;
        address.district = model.value.district;
        address.ward = model.value.ward;
    }
})

const key = ref('');
const placeholder = ref('');
const visible = ref(false);
const activeIndex = ref(0);

const address = reactive({
    city: null,
    district: null,
    ward: null
});

const onFocusInput = (event) => {

    if(address.city && address.district && address.ward){
        placeholder.value = key.value;
        key.value = null;
        activeIndex.value = 0;
    }
}

const cities = ref(citiesData.data);
const onChangeCity = (event) => {
    if(address.city){
        key.value = `${address.city.name}, `;
        districts.value = event.value.districts;
        address.district = null;
        address.ward = null;
        activeIndex.value++
    }
    
}

const districts = ref([]);
const onChangeDistrict = (event) => {
    if(address.district){
        key.value = `${address.city.name}, ${address.district.name}, `;
        wards.value = event.value.wards;
        address.ward = null;
        activeIndex.value++
    }
}

const wards = ref([]);
const onChangeWard = (event) => {
    if(address.ward){
        key.value = `${address.city.name}, ${address.district.name}, ${address.ward.name}`;
        model.value = {
            city: address.city.name,
            district: address.district.name,
            ward: address.ward.name
        };
        visible.value = false;
    }
}

const target = ref();
onClickOutside(target, event => {
    
    if(!visible.value){
        return;
    }

    if(!address.city || !address.district || !address.ward){
        key.value = null;
        placeholder.value = null;
        address.city = null;
        address.district = null;
        address.ward = null;
        activeIndex.value = 0;
        model.value = null;
    }
    else{

        key.value = `${address.city.name}, ${address.district.name}, ${address.ward.name}`;
        model.value = {
            city: address.city.name,
            district: address.district.name,
            ward: address.ward.name
        }
        
    }
    visible.value = false;
})

const ptTabview = {
    panelContainer: {
        class: 'p-0'
    }
}

</script>

<style scoped>
.tab-view{
    transform: translateY(5px);
    background-color: #fff;
    border-radius: 4px;
    box-shadow: 0px 2px 4px rgba(0, 0, 0, 0.1);
    overflow-y: auto;
}

</style>
