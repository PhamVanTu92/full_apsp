import { ref, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/Pinia/auth'
import { useRouter, useRoute } from 'vue-router'
import { getTimeoutConfig } from '@/config/idleTimeout.config'

export function useIdleTimeout(customTimeoutMinutes = null) {
  const authStore = useAuthStore()
  const router = useRouter()
  const route = useRoute()

  const config = ref(null)
  const isIdle = ref(false)
  const timeoutId = ref(null)
  const warningTimeoutId = ref(null)
  const showWarning = ref(false)
  const remainingTime = ref(0)
  const warningInterval = ref(null)

  // Khởi tạo config
  const initConfig = async () => {
    config.value = await getTimeoutConfig()
  }

  // Thời gian timeout (phút -> milliseconds)
  const getTimeoutDuration = () => {
    if (!config.value) return 30 * 60 * 1000 // default 30 phút
    const timeoutMinutes = customTimeoutMinutes || config.value.defaultTimeoutMinutes
    return timeoutMinutes * 60 * 1000
  }

  const getWarningDuration = () => {
    if (!config.value) return 1 * 60 * 1000 // default 1 phút
    return config.value.warningBeforeLogoutMinutes * 60 * 1000
  }

  // Kiểm tra nếu route hiện tại bị loại trừ
  const isExcludedRoute = () => {
    if (!config.value) return false
    return config.value.excludedRoutes.some(excludedRoute =>
      route.path.startsWith(excludedRoute)
    )
  }

  // Kiểm tra nếu tính năng được bật
  const isEnabled = () => {
    if (!config.value) return false
    return config.value.enabled && authStore.isLoggedIn && !isExcludedRoute()
  }

  // Các sự kiện user interaction
  const getTrackedEvents = () => {
    if (!config.value) return ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click', 'keydown']
    return config.value.trackedEvents
  }

  // Reset timer khi có activity
  const resetTimer = () => {
    clearTimeout(timeoutId.value)
    clearTimeout(warningTimeoutId.value)
    clearInterval(warningInterval.value)

    showWarning.value = false
    isIdle.value = false

    // Chỉ start timer nếu được enabled
    if (isEnabled()) {
      startTimer()
    }
  }

  // Bắt đầu timer
  const startTimer = () => {
    const TIMEOUT_DURATION = getTimeoutDuration()
    const WARNING_DURATION = getWarningDuration()

    // Timer cảnh báo (trước timeout 2 phút)
    warningTimeoutId.value = setTimeout(() => {
      showWarningDialog()
    }, TIMEOUT_DURATION - WARNING_DURATION)

    // Timer logout chính
    timeoutId.value = setTimeout(() => {
      performLogout()
    }, TIMEOUT_DURATION)
  }

  // Hiển thị dialog cảnh báo
  const showWarningDialog = () => {
    showWarning.value = true
    remainingTime.value = getWarningDuration() / 1000 // Convert to seconds

    // Countdown cho warning
    warningInterval.value = setInterval(() => {
      remainingTime.value--
      if (remainingTime.value <= 0) {
        clearInterval(warningInterval.value)
        performLogout()
      }
    }, 1000)
  }

  // Thực hiện logout
  const performLogout = () => {
    clearAllTimers()
    isIdle.value = true

    // Dispatch idle logout action
    authStore.idleLogout()
    router.push({ name: 'login' })
  }

  // Dọn dẹp tất cả timers
  const clearAllTimers = () => {
    clearTimeout(timeoutId.value)
    clearTimeout(warningTimeoutId.value)
    clearInterval(warningInterval.value)
    showWarning.value = false
  }

  // Extend session khi user click "Tiếp tục"
  const extendSession = () => {
    clearTimeout(warningTimeoutId.value)
    clearInterval(warningInterval.value)
    showWarning.value = false
    resetTimer()
  }

  // Khởi tạo event listeners
  const initEventListeners = () => {
    const events = getTrackedEvents()
    events.forEach(event => {
      document.addEventListener(event, resetTimer, true)
    })
  }

  // Dọn dẹp event listeners
  const removeEventListeners = () => {
    const events = getTrackedEvents()
    events.forEach(event => {
      document.removeEventListener(event, resetTimer, true)
    })
  }

  // Lifecycle hooks
  onMounted(async () => {
    await initConfig()
    initEventListeners()
    // Chỉ start timer nếu được enabled
    if (isEnabled()) {
      resetTimer()
    }
  })

  onUnmounted(() => {
    removeEventListeners()
    clearAllTimers()
  })

  return {
    isIdle,
    showWarning,
    remainingTime,
    extendSession,
    resetTimer,
    clearAllTimers,
    config
  }
}