export class SettingModel {
    id!: number;
    userType!: string;
    is2FARequired!: boolean;
    twoFactorType!: 'Email';
    timeout!: number;
    sessionTime!: number;
}
