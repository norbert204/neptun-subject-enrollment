local courseKey = KEYS[1]
local enrollmentCountKey = KEYS[2]
local maxEnrollmentKey = KEYS[3]
local studentId = ARGV[1]

-- Ellenõrizzük, hogy a diák már felvette-e a tárgyat
if redis.call('SISMEMBER', courseKey, studentId) == 1 then
    return {0, 'already_registered'}
end

-- Ellenõrizzük a létszámkorlátot
local currentEnrollment = tonumber(redis.call('GET', enrollmentCountKey) or '0')
local maxEnrollment = tonumber(redis.call('GET', maxEnrollmentKey) or '10')

if currentEnrollment >= maxEnrollment then
    return {0, 'course_full'}
end

-- Növeljük a létszámot és hozzáadjuk a diákot
redis.call('INCR', enrollmentCountKey)
redis.call('SADD', courseKey, studentId)

return {1, 'success'}
