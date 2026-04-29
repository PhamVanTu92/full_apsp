<template>
  <Dialog v-model:visible="visible" modal header="Tạo mới nhóm nhà cung cấp" :style="{ width: '25rem' }">
        
        <InputText type="text" v-model="value" />
        
        <template #footer>
            <Button label="lưu" @click="onSave"/>
        </template>
  </Dialog>
  
</template>

<script setup>
import {ref, onBeforeMount} from 'vue';
const emits = defineEmits(['onSave'])

const visible = defineModel('visible', {
    default: false
}) 

const props = defineProps({
    //Dữ liệu của 
    payload: {
        type: Object,
        default: null
    }
})

//Gửi API để lưu nhóm nhà phân phối (POST + PUT)
const onSave = () => {
    emits('onSave', null)
}

onBeforeMount(() =>{
    if(props.payload?.id){
        alert(1)
    }
})

const value = ref('')

</script>

<style>

</style>
