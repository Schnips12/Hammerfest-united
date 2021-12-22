import json
from inspect import getsourcefile
from os.path import abspath
from pathlib import Path

currentPath = Path(abspath(getsourcefile(lambda:0)))
name = "adventure"
xmlName = name+".xml"
jsonName = name+".json"
targetPath = currentPath.parent.parent / "hammerfest" / "xml" / "levels" / xmlName
SavingPath = currentPath.parent / "Assets" / "json" / "levels" / jsonName

adventure = targetPath.open()
adventureThingy = adventure.read().split(":")
adventure.close()

#BitCodec variables
fast = False
fl_read	= []
error_flag = False
nbits : int = 0
bits : int = 0
crc : int = 0
in_pos : int = 0

#PersistCodec variables
fieldtbl = []
nfields : int = 0
nfields_bits : int = 0
next_field_bits : int = 1
data = ""
cache = []
result = None


def c64(code):
    chars = "$aA_"
    if( code < 0 ):
        return "?"
    if( code < 26 ):
        return chr(code+ord(chars[1]))
    if( code < 52 ):
        return chr((code-26)+ord(chars[2]))
    if( code < 62 ):
        return chr((code-52)+ord("0"))
    if( code == 62 ):
        return "-"
    if( code == 63 ):
        return chars[3]
    return "?"

def write(n,b):
    global nbits
    global bits
    global crc
    global data
    nbits += n
    bits <<= n
    bits |= b
    while(nbits >= 6):
        nbits -= 6
        k = (bits >> nbits) & 63
        crc ^= k
        crc &= 0xFFFFFF
        crc *= k
        data += c64(k)

def read(n):
    global nbits
    global bits
    global crc
    global data
    global in_pos
    while(nbits < n):
        c = d64(data[in_pos])
        in_pos += 1
        if( in_pos > len(data)):
            error_flag = True
            return -1

        crc ^= c
        crc &= 0xFFFFFF
        crc *= c
        nbits += 6
        bits <<= 6
        bits |= c
    nbits -= n
    return (bits >> nbits) & ((1 << n) - 1)

def d64(code):
    chars = "$azAZ_" # anti obfu
    if( code >= chars[1] and code <= chars[2] ):
        return ord(code) - ord(chars[1])
    if( code >= chars[3] and code <= chars[4] ):
        return ord(code) - ord(chars[3]) + 26
    if( code >= "0" and code <= "9" ):
        return ord(code) - ord("0") + 52
    if( code == "-"):
        return 62
    if( code == chars[5] ):
        return 63
    return None


def setData(d):
    global error_flag
    global data
    global in_pos
    global nbits
    global bits
    global crc

    error_flag = False
    data = d
    in_pos = 0
    nbits = 0
    bits = 0
    crc = 0

def decodeInit(data):
    global fieldtbl
    global fast
    global nfields
    global next_field_bits
    global nfields_bits
    global cache
    global result

    fast = False
    setData(data)
    fieldtbl = []
    nfields = 0
    next_field_bits = 1
    nfields_bits = 0
    cache = []
    result = None

def decode_int():
    nbits = read(2)
    if( nbits == 3 ):
        is_neg = (read(1) == 1)
        if( is_neg ):
            return -decode_int()
        is_big = (read(1) == 1)
        if( is_big ):
            n = read(16)
            n2 = read(16)
            return n | (n2 << 16)
        else:
            return read(16)
    i = read((nbits+1)*2)
    return i

def decode_float():
    l = read(5)
    s = ""
    for i in range(l):
        k = read(4)
        if( k < 10 ):
            k += 48
        else:
            match k:
                case 10:
                    k = 46
                case 11:
                    k = 43
                case 12:
                    k = 45
                case _:
                    k = 101
        
        s += chr(k)
    return s

def decode_object():
    o = dict()
    cache.insert(0, o)
    return o

def decode_object_field(o):
    global fieldtbl
    global nfields
    global nfields_bits
    global next_field_bits
    is_field_index = (read(1) == 0)
    if(is_field_index):
        k = fieldtbl[read(nfields_bits)]
    else:
        is_end = (read(1) == 1)
        if(is_end):
            return False
        k = decode_string()
        if(k[0] == "$"): #obfu_mode
            k = k[1:]
        nfields += 1
        fieldtbl.insert(nfields, k)
        if( nfields >= next_field_bits ):
            nfields_bits += 1
            next_field_bits *= 2
    o[k] = do_decode()
    return True

def decode_string():
    len = decode_int()
    is_b64 = (read(1) == 0)
    s = ""
    if(is_b64):
        for i in range(len):
            s += c64(read(6))
    else:
        is_ascii = (read(1) == 0)
        for i in range(len):
            match is_ascii:
                case True:
                    s += chr(read(7))
                case False:
                    s += chr(read(8))
    return s

def decode_array_item(a):
    elt = (read(1) == 0)
    if(elt):
        a.append(do_decode())
    else:
        exit = (read(1) == 1)
        if(exit):
            return False
        else:
            for index in decode_int():
                a.append("filler") #no clue what this line does
    return True

def decode_array():
    global cache
    a = []
    cache.insert(0, a)
    return a

def do_decode():
    is_number = (read(1) == 0)
    if(is_number):
        is_float = (read(1) == 1)
        if(is_float):
            is_special = (read(1) == 1)
            if(is_special):
                is_infinity = (read(1) == 1)
                if(is_infinity):
                    is_negative = (read(1) == 1)
                    if( is_negative ):
                        return float("-inf")
                    else:
                        return float("inf")
                else:
                    return None
            else:
                return decode_float()
        else:
            return decode_int()
    is_array_obj = (read(1) == 0)
    if(is_array_obj):
        is_obj = (read(1) == 1)
        if(is_obj):
            return decode_object()
        else:
            return decode_array()
    tflag = read(2)
    if(tflag == 0):
        return False
    elif(tflag == 1):
        return True
    elif(tflag == 2):
        return decode_string()
    else:
        return None


def c64(code):
    chars = "$aA_"
    if( code < 0 ):
        return "?"
    if( code < 26 ):
        return chr(code+ord(chars[1]))
    if( code < 52 ):
        return chr((code-26)+ord(chars[2]))
    if( code < 62 ):
        return chr((code-52)+ord("0"))
    if( code == 62 ):
        return "-"
    if( code == 63 ):
        return chars[3]
    return "?"


#magic shit happening here
def crcStr():
    global crc
    return c64(crc&63) + c64((crc>>6)&63) + c64((crc>>12)&63) + c64((crc>>18)&63)


def decodeEnd():
    global result
    global data

    if(False): #TODO fix this
        s = crcStr()
        s2 = data[in_pos:4]
        if( s != s2 ):
            return None
    return result



def decodeLoop():
    global result

    if(len(cache) == 0 ):
        result = do_decode()
    else:
        o = cache[0]
        if(isinstance(o, list)):
            if(not decode_array_item(o)):
                cache.pop(0)
        else:
            if(not decode_object_field(o)):
                cache.pop(0)
    if(False): #error_flag
        result = None
        return False
    return (len(cache) != 0)

def decode(data):
    decodeInit(data)
    while(decodeLoop()):
        pass
    return decodeEnd()


class BadData:
	id : int
	x : int
	y : int

	def __init__(self, id, x, y):
		self.id	= id
		self.x	= x
		self.y	= y

def unserialize(id):
    global fl_read

    l = decode(adventureThingy[id])
    if (False): #fl_mirror
        l = flip(l)
    #convertWalls(l)
    #if (l.specialSlots == None or l.scoreSlots == None):
    #    print("empty slot array found ! spec="+l.specialSlots.length+" score="+l.scoreSlots.length)
    #fl_read[id] = True
    return l


def JSONSerializer(obj):
    if hasattr(obj, '__dict__'):
        return obj.__dict__
    else:
        return obj


index = 0
toJson = ""
for level in adventureThingy:
    level = unserialize(index)

    nestedColumn = dict()
    columnIndex = 0
    for column in level["map"]:
        reversedColumn = column[::-1]
        nestedColumn["column"] = reversedColumn
        level["map"][columnIndex] = nestedColumn.copy()
        columnIndex += 1
    levelString = json.dumps(level, default=JSONSerializer)
    toJson += levelString + ";"


adventureJson = SavingPath.open("w+")
adventureJson.write(toJson)
adventureJson.close()