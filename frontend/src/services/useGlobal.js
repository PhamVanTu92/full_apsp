// src/composables/useGlobal.js
import { getCurrentInstance } from 'vue';
import { useToast } from 'primevue/usetoast'; // Hoặc tùy thuộc vào thư viện bạn đang dùng

export function useGlobal() {
  const { appContext } = getCurrentInstance();
  const toast = useToast();
  const FunctionGlobal = appContext.config.globalProperties;

  return {
    toast,
    FunctionGlobal
  };
}