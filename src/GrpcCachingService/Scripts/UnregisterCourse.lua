local courseKey = KEYS[1]
local enrollmentCountKey = KEYS[2]
local studentId = ARGV[1]

-- Ellenõrizzük, hogy a diák valóban felvette-e a tárgyat
if redis.call('SISMEMBER', courseKey, studentId) == 0 then
    return 0
end

-- Csökkentjük a létszámot és eltávolítjuk a diákot
local currentValue = tonumber(redis.call('GET', enrollmentCountKey) or '0')
if currentValue > 0 then
    redis.call('DECR', enrollmentCountKey)
end

redis.call('SREM', courseKey, studentId)

return 1
