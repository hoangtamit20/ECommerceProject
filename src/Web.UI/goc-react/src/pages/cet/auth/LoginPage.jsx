import React, { useState, useEffect } from 'react';
import { ApiClient } from '../../../services/auth/base/APIClient';
import { useNavigate } from 'react-router-dom';
import { LocalStorageService } from '../../../services/auth/base/LocalStorageService';
import { toast } from 'react-toastify';
import './css/loginpage.css';
import LoginImage from '../../../assets/images/auth/Login.png';
import LoadingApi from '../../../components/base/LoadingApi';
import FormErrorAlert from '../../../components/base/FormErrorAlert';
import { APIEndpoint } from '../../../services/auth/base/APIEndpoints';

const LoginPage = () => {
    const [loginModel, setLoginModel] = useState({ email: '', password: '' });
    const [loading, setLoading] = useState(false);
    const [errors, setErrors] = useState([]);
    const [levelClass, setLevelClass] = useState('');

    const navigate = useNavigate();

    useEffect(() => {
        document.title = "Đăng nhập";
        const token = LocalStorageService.getToken();
        console.log(token);
        if (token) {
            navigate(LocalStorageService.getPreviousUrl() || '/');
        } else {
            LocalStorageService.setPreviousUrl(window.location.pathname);
        }

        if (location.state && location.state.message) {
            toast.info(location.state.message);
            location.state.message = '';
        }
    }, [navigate]);


    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setLoginModel({ ...loginModel, [name]: value });
    };

    const handleLogin = async (e) => {
        e.preventDefault();
        setLoading(true);
        setErrors([]);

        try {
            const response = await ApiClient.post(APIEndpoint.CET_Auth_System_Login, loginModel);
            setLoading(false);
            if (response) {
                const data = response.data;
                if (data.success) {
                    if (data.data.twoFactorEnabled) {
                        navigate(`/xac-thuc-hai-yeu-to?email=${encodeURIComponent(loginModel.email)}`);
                    } else {
                        const tokens = {
                            accessToken: data.data.accessToken,
                            refreshToken: data.data.refreshToken
                        };
                        LocalStorageService.setToken(JSON.stringify(tokens));
                        navigate(LocalStorageService.getPreviousUrl() || '/');
                    }
                } else {
                    setErrors(data.errors || []);
                }
            }
            else {
                toast.error("Đã có lỗi xảy ra.")
            }
        } catch (error) {
            toast.error(error.message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <section className="p-3 p-md-4 p-xl-5">
                <div className="container">
                    <div className="card border-light-subtle shadow-sm" style={{ borderRadius: '25px', backgroundColor: 'rgba(255, 255, 255, 0.49)' }}>
                        <div className="row g-0">
                            <div className="col-12 col-md-6">
                                <img className="img-fluid rounded-start w-100 h-100 object-fit-cover" src={LoginImage} alt="BootstrapBrain Logo" />
                            </div>
                            <div className="col-12 col-md-6">
                                <div className="card-body p-3 p-md-4 p-xl-5">
                                    <div className="row">
                                        <div className="col-12 mb-5">
                                            <h3>Đăng nhập</h3>
                                        </div>
                                    </div>
                                    <form onSubmit={handleLogin}>
                                        <FormErrorAlert errors={errors} levelClass={levelClass} />
                                        <div className="row gy-3 gy-md-4 overflow-hidden">
                                            <div className="col-12">
                                                <label htmlFor="email" className="form-label">Email <span className="text-danger">*</span></label>
                                                <input
                                                    type="text"
                                                    className="form-control"
                                                    name="email"
                                                    id="email"
                                                    placeholder="name@microsoft.com"
                                                    value={loginModel.email}
                                                    onChange={handleInputChange}
                                                />
                                                {errors.find(error => error.field === "Email_Error") &&
                                                    <div className="text-danger">{errors.find(error => error.field === "Email_Error").error}</div>
                                                }
                                            </div>
                                            <div className="col-12">
                                                <label htmlFor="password" className="form-label">Mật khẩu <span className="text-danger">*</span></label>
                                                <input
                                                    type="password"
                                                    className="form-control"
                                                    name="password"
                                                    id="password"
                                                    placeholder="Nhập mật khẩu của bạn"
                                                    value={loginModel.password}
                                                    onChange={handleInputChange}
                                                />
                                                {errors.find(error => error.field === "Password_Error") &&
                                                    <div className="text-danger">{errors.find(error => error.field === "Password_Error").error}</div>
                                                }
                                            </div>
                                            <div className="col-12">
                                                <div className="form-check">
                                                    <input className="form-check-input" type="checkbox" name="remember_me" id="remember_me" />
                                                    <label className="form-check-label text-secondary" htmlFor="remember_me">Keep me logged in</label>
                                                </div>
                                            </div>
                                            <div className="col-12">
                                                <div className="d-grid">
                                                    <button className="btn bsb-btn-xl btn-primary" type="submit" disabled={loading}>
                                                        {loading ? 'Logging in...' : 'Log in now'}
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    </form>
                                    <div className="row">
                                        <div className="col-12">
                                            <hr className="mt-5 mb-4 border-secondary-subtle" />
                                            <div className="d-flex gap-2 gap-md-4 flex-column flex-md-row justify-content-md-end">
                                                <a href="/dang-ky" className="link-secondary text-decoration-none">Tạo mới tài khoản</a>
                                                <a href="/yeu-cau-dat-lai-mat-khau" className="link-secondary text-decoration-none">Quên mật khẩu</a>
                                            </div>
                                        </div>
                                    </div>
                                    <div className="row">
                                        <div className="col-12">
                                            <p className="mt-5 mb-4">Or sign in with</p>
                                            <div className="d-flex gap-3 flex-column flex-xl-row">
                                                <a href="#!" className="btn bsb-btn-xl btn-outline-primary">
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-google" viewBox="0 0 16 16">
                                                        <path d="M15.545 6.558a9.42 9.42 0 0 1 .139 1.626c0 2.434-.87 4.492-2.384 5.885h.002C11.978 15.292 10.158 16 8 16A8 8 0 1 1 8 0a7.689 7.689 0 0 1 5.352 2.082l-2.284 2.284A4.347 4.347 0 0 0 8 3.166c-2.087 0-3.86 1.408-4.492 3.304a4.792 4.792 0 0 0 0 3.063h.003c.635 1.893 2.405 3.301 4.492 3.301 1.078 0 2.004-.276 2.722-.764h-.003a3.702 3.702 0 0 0 1.599-2.431H8v-3.08h7.545z" />
                                                    </svg>
                                                    <span className="ms-2 fs-6">Google</span>
                                                </a>
                                                <a href="#!" className="btn bsb-btn-xl btn-outline-primary">
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-facebook" viewBox="0 0 16 16">
                                                        <path d="M16 8.049c0-4.446-3.582-8.05-8-8.05C3.58 0-.002 3.603-.002 8.05c0 4.017 2.926 7.347 6.75 7.951v-5.625h-2.03V8.05H6.75V6.275c0-2.017 1.195-3.131 3.022-3.131.876 0 1.791.157 1.791.157v1.98h-1.009c-.993 0-1.303.621-1.303 1.258v1.51h2.218l-.354 2.326H9.25V16c3.824-.604 6.75-3.934 6.75-7.951z" />
                                                    </svg>
                                                    <span className="ms-2 fs-6">Facebook</span>
                                                </a>
                                                <a href="#!" className="btn bsb-btn-xl btn-outline-primary">
                                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" className="bi bi-twitter" viewBox="0 0 16 16">
                                                        <path d="M5.026 15c6.038 0 9.341-5.003 9.341-9.335 0-.143 0-.284-.01-.426A6.688 6.688 0 0 0 16 3.539a6.568 6.568 0 0 1-1.889.517A3.294 3.294 0 0 0 15.557 2a6.586 6.586 0 0 1-2.088.796A3.281 3.281 0 0 0 11.073 0a3.276 3.276 0 0 0-3.279 3.278c0 .257.03.506.085.747-2.726-.136-5.148-1.44-6.769-3.414A3.267 3.267 0 0 0 1.396 2.63a3.28 3.28 0 0 1-1.48-.408v.041c0 1.579 1.125 2.891 2.62 3.193A3.268 3.268 0 0 1 .64 5.69v.045c0 2.271 1.594 4.16 3.708 4.593a3.29 3.29 0 0 1-.864.115c-.211 0-.418-.022-.62-.058.42 1.311 1.63 2.268 3.07 2.296A6.591 6.591 0 0 1 0 13.088a9.281 9.281 0 0 0 5.034 1.477" />
                                                    </svg>
                                                    <span className="ms-2 fs-6">Twitter</span>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            {loading && (<LoadingApi />)}
        </>
    );
};

export default LoginPage;
