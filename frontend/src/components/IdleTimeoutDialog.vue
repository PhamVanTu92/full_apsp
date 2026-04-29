<template>
    <Dialog v-model:visible="dialogVisible" header="Cảnh báo phiên làm việc" modal :closable="false" class="w-30rem" :style="{ zIndex: 9999 }">
        <div class="flex align-items-center mb-3">
            <i class="pi pi-exclamation-triangle text-yellow-500 text-2xl mr-3"></i>
            <div>
                <h4 class="mb-2">{{ t('Login.sessionEnding') }}</h4>
                <p class="text-sm text-600 mb-0">{{ t('Login.warningMessage') }}</p>
            </div>
        </div>

        <div class="bg-yellow-50 border-1 border-yellow-200 border-round p-3 mb-3">
            <div class="flex align-items-center">
                <i class="pi pi-clock text-yellow-600 mr-2"></i>
                <span class="text-sm">
                    {{ t('Login.remainingTime') }}: <strong class="text-yellow-800">{{ formatTime(remainingTime) }}</strong>
                </span>
            </div>
        </div>
    </Dialog>
</template>

<script setup>
import { computed } from 'vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const props = defineProps({
    remainingTime: {
        type: Number,
        default: 0
    }
});

// Sử dụng defineModel cho v-model:visible
const dialogVisible = defineModel('visible', {
    type: Boolean,
    default: false
});

const emit = defineEmits(['extend-session', 'logout']);

// Format time từ giây thành MM:SS
const formatTime = computed(() => (seconds) => {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    return `${minutes.toString().padStart(2, '0')}:${remainingSeconds.toString().padStart(2, '0')}`;
});

const handleExtendSession = () => {
    emit('extend-session');
};

const handleLogout = () => {
    emit('logout');
};
</script>

<style scoped>
/* Custom styles nếu cần */
</style>
