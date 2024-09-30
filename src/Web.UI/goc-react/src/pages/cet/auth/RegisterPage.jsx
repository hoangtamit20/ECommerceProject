import React, { useState } from "react";
import axios from "axios";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import RegisterImage from '../../../assets/images/auth/Register.png';
import FormErrorAlert from "../../../components/base/FormErrorAlert";
import LoadingApi from "../../../components/base/LoadingApi";
import { APIEndpoint } from "../../../services/auth/base/APIEndpoints";
import { ApiClient } from "../../../services/auth/base/APIClient";

const RegisterPage = () => {
    const [registerRequestDto, setRegisterRequestDto] = useState({
        fullName: "",
        email: "",
        password: "",
        confirmPassword: "",
    });
    const [errors, setErrors] = useState([]);
    const [loading, setLoading] = useState(false);
    const [levelClass, setLevelClass] = useState('');
    const navigate = useNavigate();

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setRegisterRequestDto({ ...registerRequestDto, [name]: value });
    };

    const handleRegister = async (e) => {
        e.preventDefault();
        setLoading(true);

        try {
            const response = await ApiClient.post(APIEndpoint.CET_Auth_Register, registerRequestDto);
            setLoading(false);
            if (response.success) {
                navigate("/thong-bao", { state: response.data.data });
            } else {                
                setErrors(response.data.errors || []);
            }
        } catch (error) {
            toast.error(error.message || "Something went wrong");
        } finally {
            setLoading(false);
        }
    };

    return (
        <>
            <section className="vh-100">
                <div className="container h-100">
                    <div className="row d-flex justify-content-center align-items-center h-100">
                        <div className="col-lg-12 col-xl-11">
                            <div className="card text-black" style={{ borderRadius: "25px", backgroundColor: "rgba(255, 255, 255, 0.49)" }}>
                                <div className="card-body p-md-5">
                                    <div className="row justify-content-center">
                                        <div className="col-md-10 col-lg-6 col-xl-5 order-2 order-lg-1">
                                            <p className="text-center h1 fw-bold mb-5 mx-1 mx-md-4 mt-4">Sign up</p>

                                            <form onSubmit={handleRegister}>
                                                <FormErrorAlert errors={errors} levelClass={levelClass} />
                                                <div className="mb-3">
                                                    <label htmlFor="fullName" className="form-label text-secondary">
                                                        Họ & tên <span className="text-danger">*</span>
                                                    </label>
                                                    <div className="input-group">
                                                        <span className="input-group-text bg-white">
                                                            <i className="bi bi-person text-primary fw-bold"></i>
                                                        </span>
                                                        <input
                                                            type="text"
                                                            id="fullName"
                                                            name="fullName"
                                                            className="form-control"
                                                            placeholder="Enter your full name"
                                                            value={registerRequestDto.fullName}
                                                            onChange={handleInputChange}
                                                        />
                                                    </div>
                                                    {errors.find(error => error.field === "FullName_Error") &&
                                                        <div className="text-danger">{errors.find(error => error.field === "FullName_Error").error}</div>
                                                    }
                                                </div>

                                                <div className="mb-3">
                                                    <label htmlFor="email" className="form-label text-secondary">
                                                        Email <span className="text-danger">*</span>
                                                    </label>
                                                    <div className="input-group">
                                                        <span className="input-group-text bg-white">
                                                            <i className="bi bi-envelope text-primary fw-bold"></i>
                                                        </span>
                                                        <input
                                                            type="email"
                                                            id="email"
                                                            name="email"
                                                            className="form-control"
                                                            placeholder="Enter your email"
                                                            value={registerRequestDto.email}
                                                            onChange={handleInputChange}
                                                        />
                                                    </div>
                                                    {errors.find(error => error.field === "Email_Error") &&
                                                        <div className="text-danger">{errors.find(error => error.field === "Email_Error").error}</div>
                                                    }
                                                </div>

                                                <div className="mb-3">
                                                    <label htmlFor="password" className="form-label text-secondary">
                                                        Mật khẩu <span className="text-danger">*</span>
                                                    </label>
                                                    <div className="input-group">
                                                        <span className="input-group-text bg-white">
                                                            <i className="bi bi-lock text-primary fw-bold"></i>
                                                        </span>
                                                        <input
                                                            type="password"
                                                            id="password"
                                                            name="password"
                                                            className="form-control"
                                                            placeholder="Enter your password"
                                                            value={registerRequestDto.password}
                                                            onChange={handleInputChange}
                                                        />
                                                    </div>
                                                    {errors.filter(error => error.field === "Password_Error").map((error, index) => (
                                                        <div key={index} className="text-danger">{error.error}</div>
                                                    ))}
                                                </div>

                                                <div className="mb-3">
                                                    <label htmlFor="confirmPassword" className="form-label text-secondary">
                                                        Xác nhận mật khẩu <span className="text-danger">*</span>
                                                    </label>
                                                    <div className="input-group">
                                                        <span className="input-group-text bg-white">
                                                            <i className="bi bi-lock text-primary fw-bold"></i>
                                                        </span>
                                                        <input
                                                            type="password"
                                                            id="confirmPassword"
                                                            name="confirmPassword"
                                                            className="form-control"
                                                            placeholder="Repeat your password"
                                                            value={registerRequestDto.confirmPassword}
                                                            onChange={handleInputChange}
                                                        />
                                                    </div>
                                                    {errors.find(error => error.field === "ConfirmPassword_Error") &&
                                                        <div className="text-danger">{errors.find(error => error.field === "ConfirmPassword_Error").error}</div>
                                                    }
                                                </div>

                                                <div className="form-check d-flex justify-content-center mb-3">
                                                    <input className="form-check-input me-2" type="checkbox" id="termsOfService" />
                                                    <label className="form-check-label" htmlFor="termsOfService">
                                                        I agree all statements in <a href="#!">Terms of service</a>
                                                    </label>
                                                </div>

                                                <div className="d-flex justify-content-center mx-4 mb-3 mb-lg-4">
                                                    <button type="submit" className="btn btn-primary btn-lg" disabled={loading}>
                                                        {loading ? "Đang đăng ký..." : "Đăng ký"}
                                                    </button>
                                                </div>

                                                <div className="text-center">
                                                    <p className="mb-0">
                                                        Đã có sẵn tài khoản? <a href="/dang-nhap" className="text-primary">Trở về đăng nhập</a>
                                                    </p>
                                                </div>
                                            </form>
                                        </div>

                                        <div className="col-md-10 col-lg-6 col-xl-7 d-flex align-items-center order-1 order-lg-2">
                                            <img src={RegisterImage} className="img-fluid" alt="Sample" />
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

export default RegisterPage;
