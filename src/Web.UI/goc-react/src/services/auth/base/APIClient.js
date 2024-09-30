import { LocalStorageService } from './LocalStorageService';
import { toast } from 'react-toastify';
import { APIEndpoint } from './APIEndpoints';
import AxiosHelper from './AxiosHelper';


export const ApiClient = {
    post: async (url, data) => {
        try {
            const response = await AxiosHelper.post(url, data);
            return response;
        } catch (error) {
            console.log(error.response.data);
            if (error.response)
            {
                return error.response;
            }
            else
            {
                toast.error(error.message);
                return null;
            }
        }
    },
    get: async (url) => {
        try {
            const response = await AxiosHelper.get(url);
            return response;
        } catch (error) {
            console.log(error);
            if (error.response)
            {
                return error.response;
            }
            else
            {
                toast.error(error.message);
                return null;
            }
        }
    },
    isAuthenticated: async () => {
        const response = await ApiClient.get(APIEndpoint.CET_Auth_Authentication);
        return response && response.data !== null;
    },
    refreshToken: async () => {
        const token = LocalStorageService.getToken();
        if (token) {
            const userSession = JSON.parse(token);
            const response = await ApiClient.post(APIEndpoint.CET_Auth_RefreshToken, {
                refreshToken: userSession.refreshToken,
            });
            if (response && response.success) {
                LocalStorageService.setToken(JSON.stringify(response.data));
                return true;
            }
            LocalStorageService.removeToken();
        }
        return false;
    },
    getUserRoles: async () => {
        try {
            const response = await ApiClient.get(APIEndpoint.CET_User_Roles);
            if (response && response.success) {
                return response.roles;
            }
            return [];
        } catch (error) {
            toast.error(error);
            return [];
        }
    }
};


// Hàm xử lý lỗi API
const handleApiError = (error) => {
    if (error.response) {
        // Request made and server responded
        const status = error.response.status;
        if (status === 401) {
            toast.error("Truy cập trái phép. Vui lòng đăng nhập lại.");
            // Xử lý logout nếu cần thiết, ví dụ:
            LocalStorageService.removeToken();
            // Có thể điều hướng đến trang đăng nhập
        } else if (status === 403) {
            toast.error("Bạn không có quyền truy cập tài nguyên này.");
            // Có thể điều hướng đến trang 403
        } else {
            toast.error(error.response.data.message || "Đã xảy ra lỗi.");
        }
    } else if (error.request) {
        // The request was made but no response was received
        toast.error("Không có phản hồi từ máy chủ. Vui lòng thử lại sau.");
    } else {
        // Something happened in setting up the request
        toast.error("Error: " + error.message);
    }
};