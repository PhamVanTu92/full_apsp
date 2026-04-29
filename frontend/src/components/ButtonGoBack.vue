<template>
    <Button :label="t('body.home.back_button')" severity="secondary" @click="onClickRouterBack"/>
</template>

<script setup>
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
const { t } = useI18n();
const { router, route } = { router: useRouter(), route: useRoute() };

const onClickRouterBack = () => {
    if (route.redirectedFrom.fullPath != '/') {
        router.back();
    } else {
        router.replace('/');
    }
};
</script>

<style></style>
