import redis
import threading
import time
import random


KEY = "test"
MAX_ITEMS = 18
THREADS = 500
KEYS = ["test", "test1"]


def lua_script_test(lua, key, index):
    r = redis.Redis(host='localhost', port=6379)

    r.eval(lua, 3, key, f"{key}:count", f"{key}:max", index)

    r.close()


def queue_with_key(key, index):
    r = redis.Redis(host='localhost', port=6379)

    if r.get(f"{key}:full") == bytes("True", "UTF-8"):
        # Queue is full
        return

    if r.lpos(key, index):
        return

    i = r.rpush(key, index)

    if i > MAX_ITEMS:
        # Overflow
        return

    if i == MAX_ITEMS:
        r.set(f"{key}:full", "True")

    r.close()


    # def set_based(index):
    #     r = redis.Redis(host='localhost', port=6379)
    # 
    #     if r.sismember(KEY, index):
    #         # No
    #         return
    # 
    #     current = int(r.get(f"{KEY}:count"))
    #     
    #     if current >= MAX_ITEMS:
    #         return
    # 
    #     r.incr(f"{KEY}:count")
    #     r.sadd(KEY, index)


def benchmark_lua():
    print(80 * "-")
    print("Lua")
    print(80 * "-")

    r = redis.Redis(host='localhost', port=6379)

    luaf = open("EnrollToCourse.lua", "r")
    lua = luaf.read()
    luaf.close()

    for key in KEYS:
        r.set(f"{key}:count", 0)
        r.set(f"{key}:max", 18)

        if r.exists(key):
            r.delete(key)
            r.set(f"{key}:full", "False")

    r.close()

    threads = []
    for i in range(THREADS):
        t = threading.Thread(target=lua_script_test, args=(lua, random.choice(KEYS), i))
        threads.append(t)

    start = time.time()
    for t in threads:
        t.start()

    for t in threads:
        t.join()
    end = time.time()

    print(f"Elapsed: {end - start}")

    for key in KEYS:
        print(f"People: {r.smembers(key)}")
        print(f"Set size: {r.scard(key)}")


def benchmark_queue():
    print(80 * "-")
    print("Queue")
    print(80 * "-")

    r = redis.Redis(host='localhost', port=6379)

    for key in KEYS:
        if r.exists(key):
            r.delete(key)
            r.set(f"{key}:full", "False")

    r.close()

    threads = []
    for i in range(THREADS):
        t = threading.Thread(target=queue_with_key, args=(random.choice(KEYS), i))
        threads.append(t)

    start = time.time()
    for t in threads:
        t.start()

    for t in threads:
        t.join()
    end = time.time()

    print(f"Elapsed: {end - start}")

    for key in KEYS:
        print(f"Queue size: {r.llen(key)}")
        full = r.get(f"{key}:full")
        print(f"Queue full: {full}")
        print(f"Queue: ", r.lrange(key, 0, MAX_ITEMS - 1))


    # def benchmark_set():
    #     print(80 * "-")
    #     print("Set")
    #     print(80 * "-")
    # 
    #     r = redis.Redis(host='localhost', port=6379)
    # 
    #     r.set(f"{KEY}:count", 0)
    # 
    #     if r.exists(KEY):
    #         r.delete(KEY)
    # 
    #     r.close()
    # 
    #     threads = []
    #     for i in range(THREADS):
    #         t = threading.Thread(target=set_based, args=(i,))
    #         threads.append(t)
    # 
    #     start = time.time()
    #     for t in threads:
    #         t.start()
    # 
    #     for t in threads:
    #         t.join()
    #     end = time.time()
    # 
    #     print(f"Elapsed: {end - start}")
    # 
    #     print(f"People: {r.smembers(KEY)}")
    #     print(f"Set size: {r.scard(KEY)}")


def main():
    benchmark_queue()
    benchmark_lua()


if __name__ == "__main__":
    main()
