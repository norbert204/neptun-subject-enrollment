import axios from 'axios';

const REST_API_BASE_URL = `${process.env.GATEWAY_URI}/Subject`;

export const getEligibleCoursesForStudent = (studentId) =>
  axios.get(`${REST_API_BASE_URL}/student/${studentId}/eligible-courses`);

export const EnrollInCourse = (studentId, courseId) =>
  axios.post(`${REST_API_BASE_URL}/enroll-to-course`, {
    studentId,
    courseId
  });

export const getEnrolledCourses = (studentId) =>
  axios.get(`${REST_API_BASE_URL}/student/${studentId}/enrolled`);