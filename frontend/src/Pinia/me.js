import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import API from '../api/api-main';

// {
//     user: {
//         id: null,
//         claims: [],
//         isSupperUser: null,
//         personRoleId: null,
//         personRole: null,
//         role: null,
//         userRoles: null,
//         fullName: null,
//         phone: null,
//         userType: null,
//         note: null,
//         dob: null,
//         status: null,
//         cardId: null,
//         bpInfo: {},
//         userGroup: null,
//         lastLogin: null,
//         organizationId: null,
//         directStaff: [],
//         userName: null,
//         normalizedUserName: null,
//         email: null,
//         normalizedEmail: null,
//         phoneNumber: null
//     }
// }
export const useMeStore = defineStore('me', () => {
    const me = ref(); 
    const getMe = async () => { 
        try {
            const res = await API.get('account/me');
            me.value = res.data;
        } catch (error) {
            console.error('Failed to fetch account data:', error);
            me.value = null;
        }
        return me.value;
    };

    const computedMe = computed(()=> me.value);

    const refresh = () => {
        API.get('account/me')
            .then((res) => {
                me.value = res.data;
            })
            .catch((error) => {
                console.error('Failed to fetch account data:', error);
                me.value = null;
            });
    };
    return { me, getMe, refresh, computedMe };
});
