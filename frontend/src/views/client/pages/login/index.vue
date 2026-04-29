<script setup>
import { reactive } from 'vue';
import { useAuthStore } from '@/Pinia/auth';
import { useRouter } from 'vue-router';
import { useToast } from 'primevue/usetoast';

const toast = useToast();
const router = useRouter();
const authStore = useAuthStore();

const objForm = reactive({
    username: '',
    password: ''
});

const handleSubmit = async () => {
    const res = await authStore.login(objForm);
    if (res.status) {
        router.push({ name: 'cart' });
    } else {
        objForm.password = '';
        toast.add({ severity: 'warn', summary: 'Đăng nhập thất bại', detail: 'Message Content', life: 3000 });
    }
};
</script>

<template>
    <div class="w-full h-screen flex align-items-center justify-content-center">
        <div class="w-30rem bg-white px-5 py-5 border-round-3xl">
            <div class="mb-5 w-full text-center text-3xl font-bold">SAIGON PETRO</div>
            <form @submit.prevent="handleSubmit" class="flex flex-column">
                <label for="" class="font-medium mb-2">Tên đăng nhập</label>
                <InputText type="text" v-model="objForm.username" autocomplete="on" />
                <label for="" class="font-medium mt-3 mb-2">Mật khẩu</label>
                <InputText type="password" v-model="objForm.password" autocomplete="off" />
                <Button type="submit" class="w-full mt-5 mb-3" style="display: block">Đăng nhập</Button>
                <div class="text-right underline"><a href="#">Quên mật khẩu?</a></div>
            </form>
        </div>
    </div>
</template>
