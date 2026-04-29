import API from "@/api/api-main"
import { useUserInfoStore } from "@/Pinia/user_info/userInfo";


// Cấu hình idle timeout
export const idleTimeoutConfig = {

  // Thời gian timeout mặc định (phút)
  defaultTimeoutMinutes: 30,

  // Thời gian hiển thị cảnh báo trước khi logout (phút)
  warningBeforeLogoutMinutes: 1,

  // Các sự kiện user activity được theo dõi
  trackedEvents: [
    'mousedown',
    'mousemove',
    'keypress',
    'scroll',
    'touchstart',
    'click',
    'keydown'
  ],

  // Có bật idle timeout hay không
  enabled: true,

  // Các trang không áp dụng idle timeout
  excludedRoutes: [
    '/login',
    '/auth/otp',
    '/access-denied',
    '/error'
  ]
}

// Hàm lấy timeout config từ localStorage hoặc server
export const getTimeoutConfig = async () => {

  try {
    const userInfoStore = useUserInfoStore();
    if (!userInfoStore.userInfo) {
      await userInfoStore.initUserInfoStore()
    }
    let userType = userInfoStore.userInfo?.user.userType
    if (userType) {
      const res = await API.get(`appsetting`)
      let userSettings = res.data?.item?.find(el => el.userType === userType)
      if (userSettings) {
        return {
          ...idleTimeoutConfig,
          defaultTimeoutMinutes: userSettings.sessionTime || idleTimeoutConfig.defaultTimeoutMinutes
        }
      }
    }

  } catch (error) {
    console.warn('Could not load user timeout settings:', error)
  }

  return idleTimeoutConfig
}