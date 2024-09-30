//base
import NotFoundPage from '../../../pages/base/NotFoundPage';
import NotificationSummaryPage from '../../../pages/base/NotificationSummaryPage';
import ForbiddenPage from '../../../pages/base/ForbidenPage';
import ServerErrorPage from '../../../pages/base/ServerErrorPage';
//endbase

// auth
import LoginPage from '../../../pages/cet/auth/LoginPage';
import RegisterPage from '../../../pages/cet/auth/RegisterPage';
import RequestResetPasswordPage from '../../../pages/cet/auth/RequestResetPasswordPage';
import ConfirmResetPasswordPage from '../../../pages/cet/auth/ConfirmResetPasswordPage';
import TwoFactorAuthenticationPage from '../../../pages/cet/auth/TwoFactorAuthenticationPage';
// end auth



const routes = [
    // { path: '/admin', component: AdminPage, roles: ['Admin'] },
    // { path: '/client', component: ClientPage, roles: ['Client'] },
    // { path: '/learner', component: LearnerPage, roles: ['Learner'] },
    // { path: '/publisher', component: PublisherPage, roles: ['Publisher'] },




    // auth
    { path: '/dang-nhap', component: LoginPage, public: true },
    { path: '/dang-ky', component: RegisterPage, public: true },
    { path: '/yeu-cau-dat-lai-mat-khau', component: RequestResetPasswordPage, public: true },
    { path: '/xac-nhan-dat-lai-mat-khau', component: ConfirmResetPasswordPage, public: true },
    { path: '/xac-thuc-hai-yeu-to', component: TwoFactorAuthenticationPage, public: true },
    // end auth

    //base
    { path: '/khong-co-quyen', component: ForbiddenPage, public: true },
    { path: '/khong-tim-thay', component: NotFoundPage, public: true },
    { path: '/loi-he-thong', component: ServerErrorPage, public: true },
    { path: '/thong-bao', component: NotificationSummaryPage, public: true },
    //end base
];

export default routes;
