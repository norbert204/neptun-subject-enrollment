import axios from 'axios';

const REST_API_BASE_URL = "http://localhost:5000/Subject";

export const getEligibleCoursesForStudent = (studentId) =>
  axios.get(`${REST_API_BASE_URL}/student/${studentId}/eligible-courses`);