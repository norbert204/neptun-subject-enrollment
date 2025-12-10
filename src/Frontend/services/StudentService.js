import axios from 'axios';

// Use relative path so requests go through frontend nginx proxy (/api/* -> gateway)
const API_URL = '/api/user';

export const getAllStudents = () => {
    return axios.get(API_URL);
};

export const deleteStudent = (neptunCode) => {
    return axios.delete(`${API_URL}/${neptunCode}`);
};

export const createStudent = (studentData) => {
    return axios.post(API_URL, studentData);
};