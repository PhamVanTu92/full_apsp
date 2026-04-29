<template>
    <div class="surface-ground flex align-items-center justify-content-center min-h-screen min-w-screen overflow-hidden">
        <div class="flex flex-column align-items-center justify-content-center">
            <div style="border-radius: 56px; padding: 0.3rem; background: linear-gradient(180deg, rgba(233, 30, 99, 0.4) 10%, rgba(33, 150, 243, 0) 30%)">
                <div class="w-full surface-card py-8 px-5 sm:px-8 flex flex-column align-items-center" style="border-radius: 53px">
                    <div class="grid flex flex-column align-items-center">
                        <div class="flex justify-content-center align-items-center" style="height: 3.2rem; width: 3.2rem">
                            <a href="https://fox.ai.vn/">
                                <img src="/src/assets/images/logoFoxAImau.png" class="w-8rem" alt="logo" />
                            </a>
                        </div>
                        <h1 class="text-900 font-bold text-5xl mb-2">We're Making Things Better!</h1>
                        <span class="text-600 text-sm mb-5 mt-4">Chúng tôi đang nỗ lực để sớm ra mắt tính năng này. Vui lòng quay lại sau! </span>
                        <img src="/src/assets/images/image-12.png" alt="Error" class="mb-5" width="80%" />

                        <div class="col-span-12 mt-8 text-center">
                            <Button as="router-link" class="" severity="danger" to="/">
                                <router-link to="/" class="text-white">Quay lại</router-link>
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <AppConfig simple />
</template>

<script setup>
import AppConfig from '@/layout/AppConfig.vue';
</script>
