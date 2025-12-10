import axios from 'axios';

const API_URL = "/api/user";

export const getAllStudents = () => {
    return axios.get(API_URL);
};

export const deleteStudent = (neptunCode) => {
    return axios.delete(`${API_URL}/${neptunCode}`);
};

export const createStudent = (studentData) => {
    return axios.post(API_URL, studentData);
};