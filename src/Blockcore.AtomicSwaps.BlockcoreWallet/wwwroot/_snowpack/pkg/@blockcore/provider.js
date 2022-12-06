import { B as BlockcoreDns, W as WebRequest } from '../common/BlockcoreDns-a78571e3.js';

function createCommonjsModule(fn, basedir, module) {
	return module = {
		path: basedir,
		exports: {},
		require: function (path, base) {
			return commonjsRequire(path, (base === undefined || base === null) ? module.path : base);
		}
	}, fn(module, module.exports), module.exports;
}

function getDefaultExportFromNamespaceIfNotNamed (n) {
	return n && Object.prototype.hasOwnProperty.call(n, 'default') && Object.keys(n).length === 1 ? n['default'] : n;
}

function commonjsRequire () {
	throw new Error('Dynamic requires are not currently supported by @rollup/plugin-commonjs');
}

var global = (typeof global !== "undefined" ? global :
  typeof self !== "undefined" ? self :
  typeof window !== "undefined" ? window : {});

var lookup = [];
var revLookup = [];
var Arr = typeof Uint8Array !== 'undefined' ? Uint8Array : Array;
var inited = false;
function init () {
  inited = true;
  var code = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/';
  for (var i = 0, len = code.length; i < len; ++i) {
    lookup[i] = code[i];
    revLookup[code.charCodeAt(i)] = i;
  }

  revLookup['-'.charCodeAt(0)] = 62;
  revLookup['_'.charCodeAt(0)] = 63;
}

function toByteArray (b64) {
  if (!inited) {
    init();
  }
  var i, j, l, tmp, placeHolders, arr;
  var len = b64.length;

  if (len % 4 > 0) {
    throw new Error('Invalid string. Length must be a multiple of 4')
  }

  // the number of equal signs (place holders)
  // if there are two placeholders, than the two characters before it
  // represent one byte
  // if there is only one, then the three characters before it represent 2 bytes
  // this is just a cheap hack to not do indexOf twice
  placeHolders = b64[len - 2] === '=' ? 2 : b64[len - 1] === '=' ? 1 : 0;

  // base64 is 4/3 + up to two characters of the original data
  arr = new Arr(len * 3 / 4 - placeHolders);

  // if there are placeholders, only get up to the last complete 4 chars
  l = placeHolders > 0 ? len - 4 : len;

  var L = 0;

  for (i = 0, j = 0; i < l; i += 4, j += 3) {
    tmp = (revLookup[b64.charCodeAt(i)] << 18) | (revLookup[b64.charCodeAt(i + 1)] << 12) | (revLookup[b64.charCodeAt(i + 2)] << 6) | revLookup[b64.charCodeAt(i + 3)];
    arr[L++] = (tmp >> 16) & 0xFF;
    arr[L++] = (tmp >> 8) & 0xFF;
    arr[L++] = tmp & 0xFF;
  }

  if (placeHolders === 2) {
    tmp = (revLookup[b64.charCodeAt(i)] << 2) | (revLookup[b64.charCodeAt(i + 1)] >> 4);
    arr[L++] = tmp & 0xFF;
  } else if (placeHolders === 1) {
    tmp = (revLookup[b64.charCodeAt(i)] << 10) | (revLookup[b64.charCodeAt(i + 1)] << 4) | (revLookup[b64.charCodeAt(i + 2)] >> 2);
    arr[L++] = (tmp >> 8) & 0xFF;
    arr[L++] = tmp & 0xFF;
  }

  return arr
}

function tripletToBase64 (num) {
  return lookup[num >> 18 & 0x3F] + lookup[num >> 12 & 0x3F] + lookup[num >> 6 & 0x3F] + lookup[num & 0x3F]
}

function encodeChunk (uint8, start, end) {
  var tmp;
  var output = [];
  for (var i = start; i < end; i += 3) {
    tmp = (uint8[i] << 16) + (uint8[i + 1] << 8) + (uint8[i + 2]);
    output.push(tripletToBase64(tmp));
  }
  return output.join('')
}

function fromByteArray (uint8) {
  if (!inited) {
    init();
  }
  var tmp;
  var len = uint8.length;
  var extraBytes = len % 3; // if we have 1 byte left, pad 2 bytes
  var output = '';
  var parts = [];
  var maxChunkLength = 16383; // must be multiple of 3

  // go through the array every three bytes, we'll deal with trailing stuff later
  for (var i = 0, len2 = len - extraBytes; i < len2; i += maxChunkLength) {
    parts.push(encodeChunk(uint8, i, (i + maxChunkLength) > len2 ? len2 : (i + maxChunkLength)));
  }

  // pad the end with zeros, but make sure to not forget the extra bytes
  if (extraBytes === 1) {
    tmp = uint8[len - 1];
    output += lookup[tmp >> 2];
    output += lookup[(tmp << 4) & 0x3F];
    output += '==';
  } else if (extraBytes === 2) {
    tmp = (uint8[len - 2] << 8) + (uint8[len - 1]);
    output += lookup[tmp >> 10];
    output += lookup[(tmp >> 4) & 0x3F];
    output += lookup[(tmp << 2) & 0x3F];
    output += '=';
  }

  parts.push(output);

  return parts.join('')
}

function read (buffer, offset, isLE, mLen, nBytes) {
  var e, m;
  var eLen = nBytes * 8 - mLen - 1;
  var eMax = (1 << eLen) - 1;
  var eBias = eMax >> 1;
  var nBits = -7;
  var i = isLE ? (nBytes - 1) : 0;
  var d = isLE ? -1 : 1;
  var s = buffer[offset + i];

  i += d;

  e = s & ((1 << (-nBits)) - 1);
  s >>= (-nBits);
  nBits += eLen;
  for (; nBits > 0; e = e * 256 + buffer[offset + i], i += d, nBits -= 8) {}

  m = e & ((1 << (-nBits)) - 1);
  e >>= (-nBits);
  nBits += mLen;
  for (; nBits > 0; m = m * 256 + buffer[offset + i], i += d, nBits -= 8) {}

  if (e === 0) {
    e = 1 - eBias;
  } else if (e === eMax) {
    return m ? NaN : ((s ? -1 : 1) * Infinity)
  } else {
    m = m + Math.pow(2, mLen);
    e = e - eBias;
  }
  return (s ? -1 : 1) * m * Math.pow(2, e - mLen)
}

function write (buffer, value, offset, isLE, mLen, nBytes) {
  var e, m, c;
  var eLen = nBytes * 8 - mLen - 1;
  var eMax = (1 << eLen) - 1;
  var eBias = eMax >> 1;
  var rt = (mLen === 23 ? Math.pow(2, -24) - Math.pow(2, -77) : 0);
  var i = isLE ? 0 : (nBytes - 1);
  var d = isLE ? 1 : -1;
  var s = value < 0 || (value === 0 && 1 / value < 0) ? 1 : 0;

  value = Math.abs(value);

  if (isNaN(value) || value === Infinity) {
    m = isNaN(value) ? 1 : 0;
    e = eMax;
  } else {
    e = Math.floor(Math.log(value) / Math.LN2);
    if (value * (c = Math.pow(2, -e)) < 1) {
      e--;
      c *= 2;
    }
    if (e + eBias >= 1) {
      value += rt / c;
    } else {
      value += rt * Math.pow(2, 1 - eBias);
    }
    if (value * c >= 2) {
      e++;
      c /= 2;
    }

    if (e + eBias >= eMax) {
      m = 0;
      e = eMax;
    } else if (e + eBias >= 1) {
      m = (value * c - 1) * Math.pow(2, mLen);
      e = e + eBias;
    } else {
      m = value * Math.pow(2, eBias - 1) * Math.pow(2, mLen);
      e = 0;
    }
  }

  for (; mLen >= 8; buffer[offset + i] = m & 0xff, i += d, m /= 256, mLen -= 8) {}

  e = (e << mLen) | m;
  eLen += mLen;
  for (; eLen > 0; buffer[offset + i] = e & 0xff, i += d, e /= 256, eLen -= 8) {}

  buffer[offset + i - d] |= s * 128;
}

var toString = {}.toString;

var isArray = Array.isArray || function (arr) {
  return toString.call(arr) == '[object Array]';
};

/*!
 * The buffer module from node.js, for the browser.
 *
 * @author   Feross Aboukhadijeh <feross@feross.org> <http://feross.org>
 * @license  MIT
 */

var INSPECT_MAX_BYTES = 50;

/**
 * If `Buffer.TYPED_ARRAY_SUPPORT`:
 *   === true    Use Uint8Array implementation (fastest)
 *   === false   Use Object implementation (most compatible, even IE6)
 *
 * Browsers that support typed arrays are IE 10+, Firefox 4+, Chrome 7+, Safari 5.1+,
 * Opera 11.6+, iOS 4.2+.
 *
 * Due to various browser bugs, sometimes the Object implementation will be used even
 * when the browser supports typed arrays.
 *
 * Note:
 *
 *   - Firefox 4-29 lacks support for adding new properties to `Uint8Array` instances,
 *     See: https://bugzilla.mozilla.org/show_bug.cgi?id=695438.
 *
 *   - Chrome 9-10 is missing the `TypedArray.prototype.subarray` function.
 *
 *   - IE10 has a broken `TypedArray.prototype.subarray` function which returns arrays of
 *     incorrect length in some situations.

 * We detect these buggy browsers and set `Buffer.TYPED_ARRAY_SUPPORT` to `false` so they
 * get the Object implementation, which is slower but behaves correctly.
 */
Buffer.TYPED_ARRAY_SUPPORT = global.TYPED_ARRAY_SUPPORT !== undefined
  ? global.TYPED_ARRAY_SUPPORT
  : true;

/*
 * Export kMaxLength after typed array support is determined.
 */
var _kMaxLength = kMaxLength();

function kMaxLength () {
  return Buffer.TYPED_ARRAY_SUPPORT
    ? 0x7fffffff
    : 0x3fffffff
}

function createBuffer (that, length) {
  if (kMaxLength() < length) {
    throw new RangeError('Invalid typed array length')
  }
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    // Return an augmented `Uint8Array` instance, for best performance
    that = new Uint8Array(length);
    that.__proto__ = Buffer.prototype;
  } else {
    // Fallback: Return an object instance of the Buffer class
    if (that === null) {
      that = new Buffer(length);
    }
    that.length = length;
  }

  return that
}

/**
 * The Buffer constructor returns instances of `Uint8Array` that have their
 * prototype changed to `Buffer.prototype`. Furthermore, `Buffer` is a subclass of
 * `Uint8Array`, so the returned instances will have all the node `Buffer` methods
 * and the `Uint8Array` methods. Square bracket notation works as expected -- it
 * returns a single octet.
 *
 * The `Uint8Array` prototype remains unmodified.
 */

function Buffer (arg, encodingOrOffset, length) {
  if (!Buffer.TYPED_ARRAY_SUPPORT && !(this instanceof Buffer)) {
    return new Buffer(arg, encodingOrOffset, length)
  }

  // Common case.
  if (typeof arg === 'number') {
    if (typeof encodingOrOffset === 'string') {
      throw new Error(
        'If encoding is specified then the first argument must be a string'
      )
    }
    return allocUnsafe(this, arg)
  }
  return from(this, arg, encodingOrOffset, length)
}

Buffer.poolSize = 8192; // not used by this implementation

// TODO: Legacy, not needed anymore. Remove in next major version.
Buffer._augment = function (arr) {
  arr.__proto__ = Buffer.prototype;
  return arr
};

function from (that, value, encodingOrOffset, length) {
  if (typeof value === 'number') {
    throw new TypeError('"value" argument must not be a number')
  }

  if (typeof ArrayBuffer !== 'undefined' && value instanceof ArrayBuffer) {
    return fromArrayBuffer(that, value, encodingOrOffset, length)
  }

  if (typeof value === 'string') {
    return fromString(that, value, encodingOrOffset)
  }

  return fromObject(that, value)
}

/**
 * Functionally equivalent to Buffer(arg, encoding) but throws a TypeError
 * if value is a number.
 * Buffer.from(str[, encoding])
 * Buffer.from(array)
 * Buffer.from(buffer)
 * Buffer.from(arrayBuffer[, byteOffset[, length]])
 **/
Buffer.from = function (value, encodingOrOffset, length) {
  return from(null, value, encodingOrOffset, length)
};

if (Buffer.TYPED_ARRAY_SUPPORT) {
  Buffer.prototype.__proto__ = Uint8Array.prototype;
  Buffer.__proto__ = Uint8Array;
}

function assertSize (size) {
  if (typeof size !== 'number') {
    throw new TypeError('"size" argument must be a number')
  } else if (size < 0) {
    throw new RangeError('"size" argument must not be negative')
  }
}

function alloc (that, size, fill, encoding) {
  assertSize(size);
  if (size <= 0) {
    return createBuffer(that, size)
  }
  if (fill !== undefined) {
    // Only pay attention to encoding if it's a string. This
    // prevents accidentally sending in a number that would
    // be interpretted as a start offset.
    return typeof encoding === 'string'
      ? createBuffer(that, size).fill(fill, encoding)
      : createBuffer(that, size).fill(fill)
  }
  return createBuffer(that, size)
}

/**
 * Creates a new filled Buffer instance.
 * alloc(size[, fill[, encoding]])
 **/
Buffer.alloc = function (size, fill, encoding) {
  return alloc(null, size, fill, encoding)
};

function allocUnsafe (that, size) {
  assertSize(size);
  that = createBuffer(that, size < 0 ? 0 : checked(size) | 0);
  if (!Buffer.TYPED_ARRAY_SUPPORT) {
    for (var i = 0; i < size; ++i) {
      that[i] = 0;
    }
  }
  return that
}

/**
 * Equivalent to Buffer(num), by default creates a non-zero-filled Buffer instance.
 * */
Buffer.allocUnsafe = function (size) {
  return allocUnsafe(null, size)
};
/**
 * Equivalent to SlowBuffer(num), by default creates a non-zero-filled Buffer instance.
 */
Buffer.allocUnsafeSlow = function (size) {
  return allocUnsafe(null, size)
};

function fromString (that, string, encoding) {
  if (typeof encoding !== 'string' || encoding === '') {
    encoding = 'utf8';
  }

  if (!Buffer.isEncoding(encoding)) {
    throw new TypeError('"encoding" must be a valid string encoding')
  }

  var length = byteLength(string, encoding) | 0;
  that = createBuffer(that, length);

  var actual = that.write(string, encoding);

  if (actual !== length) {
    // Writing a hex string, for example, that contains invalid characters will
    // cause everything after the first invalid character to be ignored. (e.g.
    // 'abxxcd' will be treated as 'ab')
    that = that.slice(0, actual);
  }

  return that
}

function fromArrayLike (that, array) {
  var length = array.length < 0 ? 0 : checked(array.length) | 0;
  that = createBuffer(that, length);
  for (var i = 0; i < length; i += 1) {
    that[i] = array[i] & 255;
  }
  return that
}

function fromArrayBuffer (that, array, byteOffset, length) {
  array.byteLength; // this throws if `array` is not a valid ArrayBuffer

  if (byteOffset < 0 || array.byteLength < byteOffset) {
    throw new RangeError('\'offset\' is out of bounds')
  }

  if (array.byteLength < byteOffset + (length || 0)) {
    throw new RangeError('\'length\' is out of bounds')
  }

  if (byteOffset === undefined && length === undefined) {
    array = new Uint8Array(array);
  } else if (length === undefined) {
    array = new Uint8Array(array, byteOffset);
  } else {
    array = new Uint8Array(array, byteOffset, length);
  }

  if (Buffer.TYPED_ARRAY_SUPPORT) {
    // Return an augmented `Uint8Array` instance, for best performance
    that = array;
    that.__proto__ = Buffer.prototype;
  } else {
    // Fallback: Return an object instance of the Buffer class
    that = fromArrayLike(that, array);
  }
  return that
}

function fromObject (that, obj) {
  if (internalIsBuffer(obj)) {
    var len = checked(obj.length) | 0;
    that = createBuffer(that, len);

    if (that.length === 0) {
      return that
    }

    obj.copy(that, 0, 0, len);
    return that
  }

  if (obj) {
    if ((typeof ArrayBuffer !== 'undefined' &&
        obj.buffer instanceof ArrayBuffer) || 'length' in obj) {
      if (typeof obj.length !== 'number' || isnan(obj.length)) {
        return createBuffer(that, 0)
      }
      return fromArrayLike(that, obj)
    }

    if (obj.type === 'Buffer' && isArray(obj.data)) {
      return fromArrayLike(that, obj.data)
    }
  }

  throw new TypeError('First argument must be a string, Buffer, ArrayBuffer, Array, or array-like object.')
}

function checked (length) {
  // Note: cannot use `length < kMaxLength()` here because that fails when
  // length is NaN (which is otherwise coerced to zero.)
  if (length >= kMaxLength()) {
    throw new RangeError('Attempt to allocate Buffer larger than maximum ' +
                         'size: 0x' + kMaxLength().toString(16) + ' bytes')
  }
  return length | 0
}

function SlowBuffer (length) {
  if (+length != length) { // eslint-disable-line eqeqeq
    length = 0;
  }
  return Buffer.alloc(+length)
}
Buffer.isBuffer = isBuffer;
function internalIsBuffer (b) {
  return !!(b != null && b._isBuffer)
}

Buffer.compare = function compare (a, b) {
  if (!internalIsBuffer(a) || !internalIsBuffer(b)) {
    throw new TypeError('Arguments must be Buffers')
  }

  if (a === b) return 0

  var x = a.length;
  var y = b.length;

  for (var i = 0, len = Math.min(x, y); i < len; ++i) {
    if (a[i] !== b[i]) {
      x = a[i];
      y = b[i];
      break
    }
  }

  if (x < y) return -1
  if (y < x) return 1
  return 0
};

Buffer.isEncoding = function isEncoding (encoding) {
  switch (String(encoding).toLowerCase()) {
    case 'hex':
    case 'utf8':
    case 'utf-8':
    case 'ascii':
    case 'latin1':
    case 'binary':
    case 'base64':
    case 'ucs2':
    case 'ucs-2':
    case 'utf16le':
    case 'utf-16le':
      return true
    default:
      return false
  }
};

Buffer.concat = function concat (list, length) {
  if (!isArray(list)) {
    throw new TypeError('"list" argument must be an Array of Buffers')
  }

  if (list.length === 0) {
    return Buffer.alloc(0)
  }

  var i;
  if (length === undefined) {
    length = 0;
    for (i = 0; i < list.length; ++i) {
      length += list[i].length;
    }
  }

  var buffer = Buffer.allocUnsafe(length);
  var pos = 0;
  for (i = 0; i < list.length; ++i) {
    var buf = list[i];
    if (!internalIsBuffer(buf)) {
      throw new TypeError('"list" argument must be an Array of Buffers')
    }
    buf.copy(buffer, pos);
    pos += buf.length;
  }
  return buffer
};

function byteLength (string, encoding) {
  if (internalIsBuffer(string)) {
    return string.length
  }
  if (typeof ArrayBuffer !== 'undefined' && typeof ArrayBuffer.isView === 'function' &&
      (ArrayBuffer.isView(string) || string instanceof ArrayBuffer)) {
    return string.byteLength
  }
  if (typeof string !== 'string') {
    string = '' + string;
  }

  var len = string.length;
  if (len === 0) return 0

  // Use a for loop to avoid recursion
  var loweredCase = false;
  for (;;) {
    switch (encoding) {
      case 'ascii':
      case 'latin1':
      case 'binary':
        return len
      case 'utf8':
      case 'utf-8':
      case undefined:
        return utf8ToBytes(string).length
      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return len * 2
      case 'hex':
        return len >>> 1
      case 'base64':
        return base64ToBytes(string).length
      default:
        if (loweredCase) return utf8ToBytes(string).length // assume utf8
        encoding = ('' + encoding).toLowerCase();
        loweredCase = true;
    }
  }
}
Buffer.byteLength = byteLength;

function slowToString (encoding, start, end) {
  var loweredCase = false;

  // No need to verify that "this.length <= MAX_UINT32" since it's a read-only
  // property of a typed array.

  // This behaves neither like String nor Uint8Array in that we set start/end
  // to their upper/lower bounds if the value passed is out of range.
  // undefined is handled specially as per ECMA-262 6th Edition,
  // Section 13.3.3.7 Runtime Semantics: KeyedBindingInitialization.
  if (start === undefined || start < 0) {
    start = 0;
  }
  // Return early if start > this.length. Done here to prevent potential uint32
  // coercion fail below.
  if (start > this.length) {
    return ''
  }

  if (end === undefined || end > this.length) {
    end = this.length;
  }

  if (end <= 0) {
    return ''
  }

  // Force coersion to uint32. This will also coerce falsey/NaN values to 0.
  end >>>= 0;
  start >>>= 0;

  if (end <= start) {
    return ''
  }

  if (!encoding) encoding = 'utf8';

  while (true) {
    switch (encoding) {
      case 'hex':
        return hexSlice(this, start, end)

      case 'utf8':
      case 'utf-8':
        return utf8Slice(this, start, end)

      case 'ascii':
        return asciiSlice(this, start, end)

      case 'latin1':
      case 'binary':
        return latin1Slice(this, start, end)

      case 'base64':
        return base64Slice(this, start, end)

      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return utf16leSlice(this, start, end)

      default:
        if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
        encoding = (encoding + '').toLowerCase();
        loweredCase = true;
    }
  }
}

// The property is used by `Buffer.isBuffer` and `is-buffer` (in Safari 5-7) to detect
// Buffer instances.
Buffer.prototype._isBuffer = true;

function swap (b, n, m) {
  var i = b[n];
  b[n] = b[m];
  b[m] = i;
}

Buffer.prototype.swap16 = function swap16 () {
  var len = this.length;
  if (len % 2 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 16-bits')
  }
  for (var i = 0; i < len; i += 2) {
    swap(this, i, i + 1);
  }
  return this
};

Buffer.prototype.swap32 = function swap32 () {
  var len = this.length;
  if (len % 4 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 32-bits')
  }
  for (var i = 0; i < len; i += 4) {
    swap(this, i, i + 3);
    swap(this, i + 1, i + 2);
  }
  return this
};

Buffer.prototype.swap64 = function swap64 () {
  var len = this.length;
  if (len % 8 !== 0) {
    throw new RangeError('Buffer size must be a multiple of 64-bits')
  }
  for (var i = 0; i < len; i += 8) {
    swap(this, i, i + 7);
    swap(this, i + 1, i + 6);
    swap(this, i + 2, i + 5);
    swap(this, i + 3, i + 4);
  }
  return this
};

Buffer.prototype.toString = function toString () {
  var length = this.length | 0;
  if (length === 0) return ''
  if (arguments.length === 0) return utf8Slice(this, 0, length)
  return slowToString.apply(this, arguments)
};

Buffer.prototype.equals = function equals (b) {
  if (!internalIsBuffer(b)) throw new TypeError('Argument must be a Buffer')
  if (this === b) return true
  return Buffer.compare(this, b) === 0
};

Buffer.prototype.inspect = function inspect () {
  var str = '';
  var max = INSPECT_MAX_BYTES;
  if (this.length > 0) {
    str = this.toString('hex', 0, max).match(/.{2}/g).join(' ');
    if (this.length > max) str += ' ... ';
  }
  return '<Buffer ' + str + '>'
};

Buffer.prototype.compare = function compare (target, start, end, thisStart, thisEnd) {
  if (!internalIsBuffer(target)) {
    throw new TypeError('Argument must be a Buffer')
  }

  if (start === undefined) {
    start = 0;
  }
  if (end === undefined) {
    end = target ? target.length : 0;
  }
  if (thisStart === undefined) {
    thisStart = 0;
  }
  if (thisEnd === undefined) {
    thisEnd = this.length;
  }

  if (start < 0 || end > target.length || thisStart < 0 || thisEnd > this.length) {
    throw new RangeError('out of range index')
  }

  if (thisStart >= thisEnd && start >= end) {
    return 0
  }
  if (thisStart >= thisEnd) {
    return -1
  }
  if (start >= end) {
    return 1
  }

  start >>>= 0;
  end >>>= 0;
  thisStart >>>= 0;
  thisEnd >>>= 0;

  if (this === target) return 0

  var x = thisEnd - thisStart;
  var y = end - start;
  var len = Math.min(x, y);

  var thisCopy = this.slice(thisStart, thisEnd);
  var targetCopy = target.slice(start, end);

  for (var i = 0; i < len; ++i) {
    if (thisCopy[i] !== targetCopy[i]) {
      x = thisCopy[i];
      y = targetCopy[i];
      break
    }
  }

  if (x < y) return -1
  if (y < x) return 1
  return 0
};

// Finds either the first index of `val` in `buffer` at offset >= `byteOffset`,
// OR the last index of `val` in `buffer` at offset <= `byteOffset`.
//
// Arguments:
// - buffer - a Buffer to search
// - val - a string, Buffer, or number
// - byteOffset - an index into `buffer`; will be clamped to an int32
// - encoding - an optional encoding, relevant is val is a string
// - dir - true for indexOf, false for lastIndexOf
function bidirectionalIndexOf (buffer, val, byteOffset, encoding, dir) {
  // Empty buffer means no match
  if (buffer.length === 0) return -1

  // Normalize byteOffset
  if (typeof byteOffset === 'string') {
    encoding = byteOffset;
    byteOffset = 0;
  } else if (byteOffset > 0x7fffffff) {
    byteOffset = 0x7fffffff;
  } else if (byteOffset < -0x80000000) {
    byteOffset = -0x80000000;
  }
  byteOffset = +byteOffset;  // Coerce to Number.
  if (isNaN(byteOffset)) {
    // byteOffset: it it's undefined, null, NaN, "foo", etc, search whole buffer
    byteOffset = dir ? 0 : (buffer.length - 1);
  }

  // Normalize byteOffset: negative offsets start from the end of the buffer
  if (byteOffset < 0) byteOffset = buffer.length + byteOffset;
  if (byteOffset >= buffer.length) {
    if (dir) return -1
    else byteOffset = buffer.length - 1;
  } else if (byteOffset < 0) {
    if (dir) byteOffset = 0;
    else return -1
  }

  // Normalize val
  if (typeof val === 'string') {
    val = Buffer.from(val, encoding);
  }

  // Finally, search either indexOf (if dir is true) or lastIndexOf
  if (internalIsBuffer(val)) {
    // Special case: looking for empty string/buffer always fails
    if (val.length === 0) {
      return -1
    }
    return arrayIndexOf(buffer, val, byteOffset, encoding, dir)
  } else if (typeof val === 'number') {
    val = val & 0xFF; // Search for a byte value [0-255]
    if (Buffer.TYPED_ARRAY_SUPPORT &&
        typeof Uint8Array.prototype.indexOf === 'function') {
      if (dir) {
        return Uint8Array.prototype.indexOf.call(buffer, val, byteOffset)
      } else {
        return Uint8Array.prototype.lastIndexOf.call(buffer, val, byteOffset)
      }
    }
    return arrayIndexOf(buffer, [ val ], byteOffset, encoding, dir)
  }

  throw new TypeError('val must be string, number or Buffer')
}

function arrayIndexOf (arr, val, byteOffset, encoding, dir) {
  var indexSize = 1;
  var arrLength = arr.length;
  var valLength = val.length;

  if (encoding !== undefined) {
    encoding = String(encoding).toLowerCase();
    if (encoding === 'ucs2' || encoding === 'ucs-2' ||
        encoding === 'utf16le' || encoding === 'utf-16le') {
      if (arr.length < 2 || val.length < 2) {
        return -1
      }
      indexSize = 2;
      arrLength /= 2;
      valLength /= 2;
      byteOffset /= 2;
    }
  }

  function read (buf, i) {
    if (indexSize === 1) {
      return buf[i]
    } else {
      return buf.readUInt16BE(i * indexSize)
    }
  }

  var i;
  if (dir) {
    var foundIndex = -1;
    for (i = byteOffset; i < arrLength; i++) {
      if (read(arr, i) === read(val, foundIndex === -1 ? 0 : i - foundIndex)) {
        if (foundIndex === -1) foundIndex = i;
        if (i - foundIndex + 1 === valLength) return foundIndex * indexSize
      } else {
        if (foundIndex !== -1) i -= i - foundIndex;
        foundIndex = -1;
      }
    }
  } else {
    if (byteOffset + valLength > arrLength) byteOffset = arrLength - valLength;
    for (i = byteOffset; i >= 0; i--) {
      var found = true;
      for (var j = 0; j < valLength; j++) {
        if (read(arr, i + j) !== read(val, j)) {
          found = false;
          break
        }
      }
      if (found) return i
    }
  }

  return -1
}

Buffer.prototype.includes = function includes (val, byteOffset, encoding) {
  return this.indexOf(val, byteOffset, encoding) !== -1
};

Buffer.prototype.indexOf = function indexOf (val, byteOffset, encoding) {
  return bidirectionalIndexOf(this, val, byteOffset, encoding, true)
};

Buffer.prototype.lastIndexOf = function lastIndexOf (val, byteOffset, encoding) {
  return bidirectionalIndexOf(this, val, byteOffset, encoding, false)
};

function hexWrite (buf, string, offset, length) {
  offset = Number(offset) || 0;
  var remaining = buf.length - offset;
  if (!length) {
    length = remaining;
  } else {
    length = Number(length);
    if (length > remaining) {
      length = remaining;
    }
  }

  // must be an even number of digits
  var strLen = string.length;
  if (strLen % 2 !== 0) throw new TypeError('Invalid hex string')

  if (length > strLen / 2) {
    length = strLen / 2;
  }
  for (var i = 0; i < length; ++i) {
    var parsed = parseInt(string.substr(i * 2, 2), 16);
    if (isNaN(parsed)) return i
    buf[offset + i] = parsed;
  }
  return i
}

function utf8Write (buf, string, offset, length) {
  return blitBuffer(utf8ToBytes(string, buf.length - offset), buf, offset, length)
}

function asciiWrite (buf, string, offset, length) {
  return blitBuffer(asciiToBytes(string), buf, offset, length)
}

function latin1Write (buf, string, offset, length) {
  return asciiWrite(buf, string, offset, length)
}

function base64Write (buf, string, offset, length) {
  return blitBuffer(base64ToBytes(string), buf, offset, length)
}

function ucs2Write (buf, string, offset, length) {
  return blitBuffer(utf16leToBytes(string, buf.length - offset), buf, offset, length)
}

Buffer.prototype.write = function write (string, offset, length, encoding) {
  // Buffer#write(string)
  if (offset === undefined) {
    encoding = 'utf8';
    length = this.length;
    offset = 0;
  // Buffer#write(string, encoding)
  } else if (length === undefined && typeof offset === 'string') {
    encoding = offset;
    length = this.length;
    offset = 0;
  // Buffer#write(string, offset[, length][, encoding])
  } else if (isFinite(offset)) {
    offset = offset | 0;
    if (isFinite(length)) {
      length = length | 0;
      if (encoding === undefined) encoding = 'utf8';
    } else {
      encoding = length;
      length = undefined;
    }
  // legacy write(string, encoding, offset, length) - remove in v0.13
  } else {
    throw new Error(
      'Buffer.write(string, encoding, offset[, length]) is no longer supported'
    )
  }

  var remaining = this.length - offset;
  if (length === undefined || length > remaining) length = remaining;

  if ((string.length > 0 && (length < 0 || offset < 0)) || offset > this.length) {
    throw new RangeError('Attempt to write outside buffer bounds')
  }

  if (!encoding) encoding = 'utf8';

  var loweredCase = false;
  for (;;) {
    switch (encoding) {
      case 'hex':
        return hexWrite(this, string, offset, length)

      case 'utf8':
      case 'utf-8':
        return utf8Write(this, string, offset, length)

      case 'ascii':
        return asciiWrite(this, string, offset, length)

      case 'latin1':
      case 'binary':
        return latin1Write(this, string, offset, length)

      case 'base64':
        // Warning: maxLength not taken into account in base64Write
        return base64Write(this, string, offset, length)

      case 'ucs2':
      case 'ucs-2':
      case 'utf16le':
      case 'utf-16le':
        return ucs2Write(this, string, offset, length)

      default:
        if (loweredCase) throw new TypeError('Unknown encoding: ' + encoding)
        encoding = ('' + encoding).toLowerCase();
        loweredCase = true;
    }
  }
};

Buffer.prototype.toJSON = function toJSON () {
  return {
    type: 'Buffer',
    data: Array.prototype.slice.call(this._arr || this, 0)
  }
};

function base64Slice (buf, start, end) {
  if (start === 0 && end === buf.length) {
    return fromByteArray(buf)
  } else {
    return fromByteArray(buf.slice(start, end))
  }
}

function utf8Slice (buf, start, end) {
  end = Math.min(buf.length, end);
  var res = [];

  var i = start;
  while (i < end) {
    var firstByte = buf[i];
    var codePoint = null;
    var bytesPerSequence = (firstByte > 0xEF) ? 4
      : (firstByte > 0xDF) ? 3
      : (firstByte > 0xBF) ? 2
      : 1;

    if (i + bytesPerSequence <= end) {
      var secondByte, thirdByte, fourthByte, tempCodePoint;

      switch (bytesPerSequence) {
        case 1:
          if (firstByte < 0x80) {
            codePoint = firstByte;
          }
          break
        case 2:
          secondByte = buf[i + 1];
          if ((secondByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0x1F) << 0x6 | (secondByte & 0x3F);
            if (tempCodePoint > 0x7F) {
              codePoint = tempCodePoint;
            }
          }
          break
        case 3:
          secondByte = buf[i + 1];
          thirdByte = buf[i + 2];
          if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0xF) << 0xC | (secondByte & 0x3F) << 0x6 | (thirdByte & 0x3F);
            if (tempCodePoint > 0x7FF && (tempCodePoint < 0xD800 || tempCodePoint > 0xDFFF)) {
              codePoint = tempCodePoint;
            }
          }
          break
        case 4:
          secondByte = buf[i + 1];
          thirdByte = buf[i + 2];
          fourthByte = buf[i + 3];
          if ((secondByte & 0xC0) === 0x80 && (thirdByte & 0xC0) === 0x80 && (fourthByte & 0xC0) === 0x80) {
            tempCodePoint = (firstByte & 0xF) << 0x12 | (secondByte & 0x3F) << 0xC | (thirdByte & 0x3F) << 0x6 | (fourthByte & 0x3F);
            if (tempCodePoint > 0xFFFF && tempCodePoint < 0x110000) {
              codePoint = tempCodePoint;
            }
          }
      }
    }

    if (codePoint === null) {
      // we did not generate a valid codePoint so insert a
      // replacement char (U+FFFD) and advance only 1 byte
      codePoint = 0xFFFD;
      bytesPerSequence = 1;
    } else if (codePoint > 0xFFFF) {
      // encode to utf16 (surrogate pair dance)
      codePoint -= 0x10000;
      res.push(codePoint >>> 10 & 0x3FF | 0xD800);
      codePoint = 0xDC00 | codePoint & 0x3FF;
    }

    res.push(codePoint);
    i += bytesPerSequence;
  }

  return decodeCodePointsArray(res)
}

// Based on http://stackoverflow.com/a/22747272/680742, the browser with
// the lowest limit is Chrome, with 0x10000 args.
// We go 1 magnitude less, for safety
var MAX_ARGUMENTS_LENGTH = 0x1000;

function decodeCodePointsArray (codePoints) {
  var len = codePoints.length;
  if (len <= MAX_ARGUMENTS_LENGTH) {
    return String.fromCharCode.apply(String, codePoints) // avoid extra slice()
  }

  // Decode in chunks to avoid "call stack size exceeded".
  var res = '';
  var i = 0;
  while (i < len) {
    res += String.fromCharCode.apply(
      String,
      codePoints.slice(i, i += MAX_ARGUMENTS_LENGTH)
    );
  }
  return res
}

function asciiSlice (buf, start, end) {
  var ret = '';
  end = Math.min(buf.length, end);

  for (var i = start; i < end; ++i) {
    ret += String.fromCharCode(buf[i] & 0x7F);
  }
  return ret
}

function latin1Slice (buf, start, end) {
  var ret = '';
  end = Math.min(buf.length, end);

  for (var i = start; i < end; ++i) {
    ret += String.fromCharCode(buf[i]);
  }
  return ret
}

function hexSlice (buf, start, end) {
  var len = buf.length;

  if (!start || start < 0) start = 0;
  if (!end || end < 0 || end > len) end = len;

  var out = '';
  for (var i = start; i < end; ++i) {
    out += toHex(buf[i]);
  }
  return out
}

function utf16leSlice (buf, start, end) {
  var bytes = buf.slice(start, end);
  var res = '';
  for (var i = 0; i < bytes.length; i += 2) {
    res += String.fromCharCode(bytes[i] + bytes[i + 1] * 256);
  }
  return res
}

Buffer.prototype.slice = function slice (start, end) {
  var len = this.length;
  start = ~~start;
  end = end === undefined ? len : ~~end;

  if (start < 0) {
    start += len;
    if (start < 0) start = 0;
  } else if (start > len) {
    start = len;
  }

  if (end < 0) {
    end += len;
    if (end < 0) end = 0;
  } else if (end > len) {
    end = len;
  }

  if (end < start) end = start;

  var newBuf;
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    newBuf = this.subarray(start, end);
    newBuf.__proto__ = Buffer.prototype;
  } else {
    var sliceLen = end - start;
    newBuf = new Buffer(sliceLen, undefined);
    for (var i = 0; i < sliceLen; ++i) {
      newBuf[i] = this[i + start];
    }
  }

  return newBuf
};

/*
 * Need to make sure that buffer isn't trying to write out of bounds.
 */
function checkOffset (offset, ext, length) {
  if ((offset % 1) !== 0 || offset < 0) throw new RangeError('offset is not uint')
  if (offset + ext > length) throw new RangeError('Trying to access beyond buffer length')
}

Buffer.prototype.readUIntLE = function readUIntLE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var val = this[offset];
  var mul = 1;
  var i = 0;
  while (++i < byteLength && (mul *= 0x100)) {
    val += this[offset + i] * mul;
  }

  return val
};

Buffer.prototype.readUIntBE = function readUIntBE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    checkOffset(offset, byteLength, this.length);
  }

  var val = this[offset + --byteLength];
  var mul = 1;
  while (byteLength > 0 && (mul *= 0x100)) {
    val += this[offset + --byteLength] * mul;
  }

  return val
};

Buffer.prototype.readUInt8 = function readUInt8 (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 1, this.length);
  return this[offset]
};

Buffer.prototype.readUInt16LE = function readUInt16LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  return this[offset] | (this[offset + 1] << 8)
};

Buffer.prototype.readUInt16BE = function readUInt16BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  return (this[offset] << 8) | this[offset + 1]
};

Buffer.prototype.readUInt32LE = function readUInt32LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return ((this[offset]) |
      (this[offset + 1] << 8) |
      (this[offset + 2] << 16)) +
      (this[offset + 3] * 0x1000000)
};

Buffer.prototype.readUInt32BE = function readUInt32BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset] * 0x1000000) +
    ((this[offset + 1] << 16) |
    (this[offset + 2] << 8) |
    this[offset + 3])
};

Buffer.prototype.readIntLE = function readIntLE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var val = this[offset];
  var mul = 1;
  var i = 0;
  while (++i < byteLength && (mul *= 0x100)) {
    val += this[offset + i] * mul;
  }
  mul *= 0x80;

  if (val >= mul) val -= Math.pow(2, 8 * byteLength);

  return val
};

Buffer.prototype.readIntBE = function readIntBE (offset, byteLength, noAssert) {
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) checkOffset(offset, byteLength, this.length);

  var i = byteLength;
  var mul = 1;
  var val = this[offset + --i];
  while (i > 0 && (mul *= 0x100)) {
    val += this[offset + --i] * mul;
  }
  mul *= 0x80;

  if (val >= mul) val -= Math.pow(2, 8 * byteLength);

  return val
};

Buffer.prototype.readInt8 = function readInt8 (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 1, this.length);
  if (!(this[offset] & 0x80)) return (this[offset])
  return ((0xff - this[offset] + 1) * -1)
};

Buffer.prototype.readInt16LE = function readInt16LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  var val = this[offset] | (this[offset + 1] << 8);
  return (val & 0x8000) ? val | 0xFFFF0000 : val
};

Buffer.prototype.readInt16BE = function readInt16BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 2, this.length);
  var val = this[offset + 1] | (this[offset] << 8);
  return (val & 0x8000) ? val | 0xFFFF0000 : val
};

Buffer.prototype.readInt32LE = function readInt32LE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset]) |
    (this[offset + 1] << 8) |
    (this[offset + 2] << 16) |
    (this[offset + 3] << 24)
};

Buffer.prototype.readInt32BE = function readInt32BE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);

  return (this[offset] << 24) |
    (this[offset + 1] << 16) |
    (this[offset + 2] << 8) |
    (this[offset + 3])
};

Buffer.prototype.readFloatLE = function readFloatLE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);
  return read(this, offset, true, 23, 4)
};

Buffer.prototype.readFloatBE = function readFloatBE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 4, this.length);
  return read(this, offset, false, 23, 4)
};

Buffer.prototype.readDoubleLE = function readDoubleLE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 8, this.length);
  return read(this, offset, true, 52, 8)
};

Buffer.prototype.readDoubleBE = function readDoubleBE (offset, noAssert) {
  if (!noAssert) checkOffset(offset, 8, this.length);
  return read(this, offset, false, 52, 8)
};

function checkInt (buf, value, offset, ext, max, min) {
  if (!internalIsBuffer(buf)) throw new TypeError('"buffer" argument must be a Buffer instance')
  if (value > max || value < min) throw new RangeError('"value" argument is out of bounds')
  if (offset + ext > buf.length) throw new RangeError('Index out of range')
}

Buffer.prototype.writeUIntLE = function writeUIntLE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    var maxBytes = Math.pow(2, 8 * byteLength) - 1;
    checkInt(this, value, offset, byteLength, maxBytes, 0);
  }

  var mul = 1;
  var i = 0;
  this[offset] = value & 0xFF;
  while (++i < byteLength && (mul *= 0x100)) {
    this[offset + i] = (value / mul) & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeUIntBE = function writeUIntBE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  byteLength = byteLength | 0;
  if (!noAssert) {
    var maxBytes = Math.pow(2, 8 * byteLength) - 1;
    checkInt(this, value, offset, byteLength, maxBytes, 0);
  }

  var i = byteLength - 1;
  var mul = 1;
  this[offset + i] = value & 0xFF;
  while (--i >= 0 && (mul *= 0x100)) {
    this[offset + i] = (value / mul) & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeUInt8 = function writeUInt8 (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 1, 0xff, 0);
  if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
  this[offset] = (value & 0xff);
  return offset + 1
};

function objectWriteUInt16 (buf, value, offset, littleEndian) {
  if (value < 0) value = 0xffff + value + 1;
  for (var i = 0, j = Math.min(buf.length - offset, 2); i < j; ++i) {
    buf[offset + i] = (value & (0xff << (8 * (littleEndian ? i : 1 - i)))) >>>
      (littleEndian ? i : 1 - i) * 8;
  }
}

Buffer.prototype.writeUInt16LE = function writeUInt16LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
  } else {
    objectWriteUInt16(this, value, offset, true);
  }
  return offset + 2
};

Buffer.prototype.writeUInt16BE = function writeUInt16BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0xffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 8);
    this[offset + 1] = (value & 0xff);
  } else {
    objectWriteUInt16(this, value, offset, false);
  }
  return offset + 2
};

function objectWriteUInt32 (buf, value, offset, littleEndian) {
  if (value < 0) value = 0xffffffff + value + 1;
  for (var i = 0, j = Math.min(buf.length - offset, 4); i < j; ++i) {
    buf[offset + i] = (value >>> (littleEndian ? i : 3 - i) * 8) & 0xff;
  }
}

Buffer.prototype.writeUInt32LE = function writeUInt32LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset + 3] = (value >>> 24);
    this[offset + 2] = (value >>> 16);
    this[offset + 1] = (value >>> 8);
    this[offset] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, true);
  }
  return offset + 4
};

Buffer.prototype.writeUInt32BE = function writeUInt32BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0xffffffff, 0);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 24);
    this[offset + 1] = (value >>> 16);
    this[offset + 2] = (value >>> 8);
    this[offset + 3] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, false);
  }
  return offset + 4
};

Buffer.prototype.writeIntLE = function writeIntLE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) {
    var limit = Math.pow(2, 8 * byteLength - 1);

    checkInt(this, value, offset, byteLength, limit - 1, -limit);
  }

  var i = 0;
  var mul = 1;
  var sub = 0;
  this[offset] = value & 0xFF;
  while (++i < byteLength && (mul *= 0x100)) {
    if (value < 0 && sub === 0 && this[offset + i - 1] !== 0) {
      sub = 1;
    }
    this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeIntBE = function writeIntBE (value, offset, byteLength, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) {
    var limit = Math.pow(2, 8 * byteLength - 1);

    checkInt(this, value, offset, byteLength, limit - 1, -limit);
  }

  var i = byteLength - 1;
  var mul = 1;
  var sub = 0;
  this[offset + i] = value & 0xFF;
  while (--i >= 0 && (mul *= 0x100)) {
    if (value < 0 && sub === 0 && this[offset + i + 1] !== 0) {
      sub = 1;
    }
    this[offset + i] = ((value / mul) >> 0) - sub & 0xFF;
  }

  return offset + byteLength
};

Buffer.prototype.writeInt8 = function writeInt8 (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 1, 0x7f, -0x80);
  if (!Buffer.TYPED_ARRAY_SUPPORT) value = Math.floor(value);
  if (value < 0) value = 0xff + value + 1;
  this[offset] = (value & 0xff);
  return offset + 1
};

Buffer.prototype.writeInt16LE = function writeInt16LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
  } else {
    objectWriteUInt16(this, value, offset, true);
  }
  return offset + 2
};

Buffer.prototype.writeInt16BE = function writeInt16BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 2, 0x7fff, -0x8000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 8);
    this[offset + 1] = (value & 0xff);
  } else {
    objectWriteUInt16(this, value, offset, false);
  }
  return offset + 2
};

Buffer.prototype.writeInt32LE = function writeInt32LE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value & 0xff);
    this[offset + 1] = (value >>> 8);
    this[offset + 2] = (value >>> 16);
    this[offset + 3] = (value >>> 24);
  } else {
    objectWriteUInt32(this, value, offset, true);
  }
  return offset + 4
};

Buffer.prototype.writeInt32BE = function writeInt32BE (value, offset, noAssert) {
  value = +value;
  offset = offset | 0;
  if (!noAssert) checkInt(this, value, offset, 4, 0x7fffffff, -0x80000000);
  if (value < 0) value = 0xffffffff + value + 1;
  if (Buffer.TYPED_ARRAY_SUPPORT) {
    this[offset] = (value >>> 24);
    this[offset + 1] = (value >>> 16);
    this[offset + 2] = (value >>> 8);
    this[offset + 3] = (value & 0xff);
  } else {
    objectWriteUInt32(this, value, offset, false);
  }
  return offset + 4
};

function checkIEEE754 (buf, value, offset, ext, max, min) {
  if (offset + ext > buf.length) throw new RangeError('Index out of range')
  if (offset < 0) throw new RangeError('Index out of range')
}

function writeFloat (buf, value, offset, littleEndian, noAssert) {
  if (!noAssert) {
    checkIEEE754(buf, value, offset, 4);
  }
  write(buf, value, offset, littleEndian, 23, 4);
  return offset + 4
}

Buffer.prototype.writeFloatLE = function writeFloatLE (value, offset, noAssert) {
  return writeFloat(this, value, offset, true, noAssert)
};

Buffer.prototype.writeFloatBE = function writeFloatBE (value, offset, noAssert) {
  return writeFloat(this, value, offset, false, noAssert)
};

function writeDouble (buf, value, offset, littleEndian, noAssert) {
  if (!noAssert) {
    checkIEEE754(buf, value, offset, 8);
  }
  write(buf, value, offset, littleEndian, 52, 8);
  return offset + 8
}

Buffer.prototype.writeDoubleLE = function writeDoubleLE (value, offset, noAssert) {
  return writeDouble(this, value, offset, true, noAssert)
};

Buffer.prototype.writeDoubleBE = function writeDoubleBE (value, offset, noAssert) {
  return writeDouble(this, value, offset, false, noAssert)
};

// copy(targetBuffer, targetStart=0, sourceStart=0, sourceEnd=buffer.length)
Buffer.prototype.copy = function copy (target, targetStart, start, end) {
  if (!start) start = 0;
  if (!end && end !== 0) end = this.length;
  if (targetStart >= target.length) targetStart = target.length;
  if (!targetStart) targetStart = 0;
  if (end > 0 && end < start) end = start;

  // Copy 0 bytes; we're done
  if (end === start) return 0
  if (target.length === 0 || this.length === 0) return 0

  // Fatal error conditions
  if (targetStart < 0) {
    throw new RangeError('targetStart out of bounds')
  }
  if (start < 0 || start >= this.length) throw new RangeError('sourceStart out of bounds')
  if (end < 0) throw new RangeError('sourceEnd out of bounds')

  // Are we oob?
  if (end > this.length) end = this.length;
  if (target.length - targetStart < end - start) {
    end = target.length - targetStart + start;
  }

  var len = end - start;
  var i;

  if (this === target && start < targetStart && targetStart < end) {
    // descending copy from end
    for (i = len - 1; i >= 0; --i) {
      target[i + targetStart] = this[i + start];
    }
  } else if (len < 1000 || !Buffer.TYPED_ARRAY_SUPPORT) {
    // ascending copy from start
    for (i = 0; i < len; ++i) {
      target[i + targetStart] = this[i + start];
    }
  } else {
    Uint8Array.prototype.set.call(
      target,
      this.subarray(start, start + len),
      targetStart
    );
  }

  return len
};

// Usage:
//    buffer.fill(number[, offset[, end]])
//    buffer.fill(buffer[, offset[, end]])
//    buffer.fill(string[, offset[, end]][, encoding])
Buffer.prototype.fill = function fill (val, start, end, encoding) {
  // Handle string cases:
  if (typeof val === 'string') {
    if (typeof start === 'string') {
      encoding = start;
      start = 0;
      end = this.length;
    } else if (typeof end === 'string') {
      encoding = end;
      end = this.length;
    }
    if (val.length === 1) {
      var code = val.charCodeAt(0);
      if (code < 256) {
        val = code;
      }
    }
    if (encoding !== undefined && typeof encoding !== 'string') {
      throw new TypeError('encoding must be a string')
    }
    if (typeof encoding === 'string' && !Buffer.isEncoding(encoding)) {
      throw new TypeError('Unknown encoding: ' + encoding)
    }
  } else if (typeof val === 'number') {
    val = val & 255;
  }

  // Invalid ranges are not set to a default, so can range check early.
  if (start < 0 || this.length < start || this.length < end) {
    throw new RangeError('Out of range index')
  }

  if (end <= start) {
    return this
  }

  start = start >>> 0;
  end = end === undefined ? this.length : end >>> 0;

  if (!val) val = 0;

  var i;
  if (typeof val === 'number') {
    for (i = start; i < end; ++i) {
      this[i] = val;
    }
  } else {
    var bytes = internalIsBuffer(val)
      ? val
      : utf8ToBytes(new Buffer(val, encoding).toString());
    var len = bytes.length;
    for (i = 0; i < end - start; ++i) {
      this[i + start] = bytes[i % len];
    }
  }

  return this
};

// HELPER FUNCTIONS
// ================

var INVALID_BASE64_RE = /[^+\/0-9A-Za-z-_]/g;

function base64clean (str) {
  // Node strips out invalid characters like \n and \t from the string, base64-js does not
  str = stringtrim(str).replace(INVALID_BASE64_RE, '');
  // Node converts strings with length < 2 to ''
  if (str.length < 2) return ''
  // Node allows for non-padded base64 strings (missing trailing ===), base64-js does not
  while (str.length % 4 !== 0) {
    str = str + '=';
  }
  return str
}

function stringtrim (str) {
  if (str.trim) return str.trim()
  return str.replace(/^\s+|\s+$/g, '')
}

function toHex (n) {
  if (n < 16) return '0' + n.toString(16)
  return n.toString(16)
}

function utf8ToBytes (string, units) {
  units = units || Infinity;
  var codePoint;
  var length = string.length;
  var leadSurrogate = null;
  var bytes = [];

  for (var i = 0; i < length; ++i) {
    codePoint = string.charCodeAt(i);

    // is surrogate component
    if (codePoint > 0xD7FF && codePoint < 0xE000) {
      // last char was a lead
      if (!leadSurrogate) {
        // no lead yet
        if (codePoint > 0xDBFF) {
          // unexpected trail
          if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
          continue
        } else if (i + 1 === length) {
          // unpaired lead
          if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
          continue
        }

        // valid lead
        leadSurrogate = codePoint;

        continue
      }

      // 2 leads in a row
      if (codePoint < 0xDC00) {
        if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
        leadSurrogate = codePoint;
        continue
      }

      // valid surrogate pair
      codePoint = (leadSurrogate - 0xD800 << 10 | codePoint - 0xDC00) + 0x10000;
    } else if (leadSurrogate) {
      // valid bmp char, but last char was a lead
      if ((units -= 3) > -1) bytes.push(0xEF, 0xBF, 0xBD);
    }

    leadSurrogate = null;

    // encode utf8
    if (codePoint < 0x80) {
      if ((units -= 1) < 0) break
      bytes.push(codePoint);
    } else if (codePoint < 0x800) {
      if ((units -= 2) < 0) break
      bytes.push(
        codePoint >> 0x6 | 0xC0,
        codePoint & 0x3F | 0x80
      );
    } else if (codePoint < 0x10000) {
      if ((units -= 3) < 0) break
      bytes.push(
        codePoint >> 0xC | 0xE0,
        codePoint >> 0x6 & 0x3F | 0x80,
        codePoint & 0x3F | 0x80
      );
    } else if (codePoint < 0x110000) {
      if ((units -= 4) < 0) break
      bytes.push(
        codePoint >> 0x12 | 0xF0,
        codePoint >> 0xC & 0x3F | 0x80,
        codePoint >> 0x6 & 0x3F | 0x80,
        codePoint & 0x3F | 0x80
      );
    } else {
      throw new Error('Invalid code point')
    }
  }

  return bytes
}

function asciiToBytes (str) {
  var byteArray = [];
  for (var i = 0; i < str.length; ++i) {
    // Node's code seems to be doing this and not & 0x7F..
    byteArray.push(str.charCodeAt(i) & 0xFF);
  }
  return byteArray
}

function utf16leToBytes (str, units) {
  var c, hi, lo;
  var byteArray = [];
  for (var i = 0; i < str.length; ++i) {
    if ((units -= 2) < 0) break

    c = str.charCodeAt(i);
    hi = c >> 8;
    lo = c % 256;
    byteArray.push(lo);
    byteArray.push(hi);
  }

  return byteArray
}


function base64ToBytes (str) {
  return toByteArray(base64clean(str))
}

function blitBuffer (src, dst, offset, length) {
  for (var i = 0; i < length; ++i) {
    if ((i + offset >= dst.length) || (i >= src.length)) break
    dst[i + offset] = src[i];
  }
  return i
}

function isnan (val) {
  return val !== val // eslint-disable-line no-self-compare
}


// the following is from is-buffer, also by Feross Aboukhadijeh and with same lisence
// The _isBuffer check is for Safari 5-7 support, because it's missing
// Object.prototype.constructor. Remove this eventually
function isBuffer(obj) {
  return obj != null && (!!obj._isBuffer || isFastBuffer(obj) || isSlowBuffer(obj))
}

function isFastBuffer (obj) {
  return !!obj.constructor && typeof obj.constructor.isBuffer === 'function' && obj.constructor.isBuffer(obj)
}

// For Node v0.10 support. Remove this eventually.
function isSlowBuffer (obj) {
  return typeof obj.readFloatLE === 'function' && typeof obj.slice === 'function' && isFastBuffer(obj.slice(0, 0))
}

var _polyfillNode_buffer = /*#__PURE__*/Object.freeze({
	__proto__: null,
	Buffer: Buffer,
	INSPECT_MAX_BYTES: INSPECT_MAX_BYTES,
	SlowBuffer: SlowBuffer,
	isBuffer: isBuffer,
	kMaxLength: _kMaxLength
});

var buffer = /*@__PURE__*/getDefaultExportFromNamespaceIfNotNamed(_polyfillNode_buffer);

var safeBuffer = createCommonjsModule(function (module, exports) {
/*! safe-buffer. MIT License. Feross Aboukhadijeh <https://feross.org/opensource> */
/* eslint-disable node/no-deprecated-api */

var Buffer = buffer.Buffer;

// alternative to using Object.keys for old browsers
function copyProps (src, dst) {
  for (var key in src) {
    dst[key] = src[key];
  }
}
if (Buffer.from && Buffer.alloc && Buffer.allocUnsafe && Buffer.allocUnsafeSlow) {
  module.exports = buffer;
} else {
  // Copy properties from require('buffer')
  copyProps(buffer, exports);
  exports.Buffer = SafeBuffer;
}

function SafeBuffer (arg, encodingOrOffset, length) {
  return Buffer(arg, encodingOrOffset, length)
}

SafeBuffer.prototype = Object.create(Buffer.prototype);

// Copy static methods from Buffer
copyProps(Buffer, SafeBuffer);

SafeBuffer.from = function (arg, encodingOrOffset, length) {
  if (typeof arg === 'number') {
    throw new TypeError('Argument must not be a number')
  }
  return Buffer(arg, encodingOrOffset, length)
};

SafeBuffer.alloc = function (size, fill, encoding) {
  if (typeof size !== 'number') {
    throw new TypeError('Argument must be a number')
  }
  var buf = Buffer(size);
  if (fill !== undefined) {
    if (typeof encoding === 'string') {
      buf.fill(fill, encoding);
    } else {
      buf.fill(fill);
    }
  } else {
    buf.fill(0);
  }
  return buf
};

SafeBuffer.allocUnsafe = function (size) {
  if (typeof size !== 'number') {
    throw new TypeError('Argument must be a number')
  }
  return Buffer(size)
};

SafeBuffer.allocUnsafeSlow = function (size) {
  if (typeof size !== 'number') {
    throw new TypeError('Argument must be a number')
  }
  return buffer.SlowBuffer(size)
};
});

/*
  info from:
    https://github.com/Bitcoin-ABC/bitcoin-abc/blob/master/src/chainparams.cpp
*/

var common = {
  name: 'BitcoinCash',
  per1: 1e8,
  unit: 'BCH'
};

var main = Object.assign({}, {
  hashGenesisBlock: '000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f',
  // nDefaultPort
  port: 8333,
  portRpc: 8332,
  protocol: {
    // pchMessageStart
    magic: 0xe8f3e1e3 // careful, sent over wire as little endian
  },
  // vSeeds
  seedsDns: [
    'seed.bitcoinabc.org',
    'seed-abc.bitcoinforks.org',
    'btccash-seeder.bitcoinunlimited.info',
    'seed.bitprim.org',
    'seed.deadalnix.me',
    'seeder.criptolayer.net'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 145,
    private: 0x80,
    public: 0x00,
    scripthash: 0x05
  }
}, common);

var test = Object.assign({}, {
  hashGenesisBlock: '000000000933ea01ad0ee984209779baaec3ced90fa3f408719526f8d77f4943',
  port: 18333,
  portRpc: 18332,
  protocol: {
    magic: 0xf4f3e5f4
  },
  seedsDns: [
    'testnet-seed.bitcoinabc.org',
    'testnet-seed-abc.bitcoinforks.org',
    'testnet-seed.bitprim.org',
    'testnet-seed.deadalnix.me',
    'testnet-seeder.criptolayer.net'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common);

var regtest = Object.assign({}, {
  hashGenesisBlock: '0f9188f13cb7b2c71f2a335e3a4fc328bf5beb436012afca590b1a11466e2206',
  port: 18444,
  portRpc: 18332,
  protocol: {
    magic: 0xfabfb5da
  },
  seedsDns: [],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common);

var bch = {
  main,
  test,
  regtest
};

/*
  info from:
    https://github.com/rat4/blackcoin/blob/master/src/chainparams.cpp
*/
var common$1 = {
  name: 'BlackCoin',
  per1: 1e8,
  unit: 'BLK'
};

var main$1 = Object.assign({}, {
  hashGenesisBlock: '000001faef25dec4fbcf906e6242621df2c183bf232f263d0ba5b101911e4563',
  port: 15714,
  portRpc: 15715,
  protocol: {
    magic: 0x05223570 // careful, sent over wire as little endian
  },
  seedsDns: [
    'rat4.blackcoin.co',
    'seed.blackcoin.co',
    'archon.darkfox.id.au',
    'foxy.seeds.darkfox.id.au',
    '6.syllabear.us.to',
    'bcseed.syllabear.us.to'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 0xa,
    private: 0x99,
    public: 0x19,
    scripthash: 0x55
  }
}, common$1);

var blk = {
  main: main$1,
  test: null
};

/*
  info from:
    https://github.com/bitcoin/bitcoin/blob/master/src/chainparams.cpp
*/

var common$2 = {
  name: 'Bitcoin',
  per1: 1e8,
  unit: 'BTC',
  messagePrefix: '\x18Bitcoin Signed Message:\n'
};

var main$2 = Object.assign({}, {
  hashGenesisBlock: '000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f',
  // nDefaultPort
  port: 8333,
  portRpc: 8332,
  protocol: {
    // pchMessageStart
    magic: 0xd9b4bef9 // careful, sent over wire as little endian
  },
  bech32: 'bc',
  // vSeeds
  seedsDns: [
    'seed.bitcoin.sipa.be',
    'dnsseed.bluematt.me',
    'seed.bitcoinstats.com',
    'seed.bitcoin.jonasschnelli.ch',
    'seed.btc.petertodd.org',
    'seed.bitcoin.sprovoost.nl',
    'dnsseed.emzy.de'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 0,
    private: 0x80,
    public: 0x00,
    scripthash: 0x05
  }
}, common$2);

var test$1 = Object.assign({}, {
  hashGenesisBlock: '000000000933ea01ad0ee984209779baaec3ced90fa3f408719526f8d77f4943',
  port: 18333,
  portRpc: 18332,
  protocol: {
    magic: 0x0709110b
  },
  bech32: 'tb',
  seedsDns: [
    'testnet-seed.alexykot.me',
    'testnet-seed.bitcoin.schildbach.de',
    'testnet-seed.bitcoin.petertodd.org',
    'testnet-seed.bluematt.me'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$2);

var regtest$1 = Object.assign({}, {
  hashGenesisBlock: '0f9188f13cb7b2c71f2a335e3a4fc328bf5beb436012afca590b1a11466e2206',
  port: 18444,
  portRpc: 18332,
  protocol: {
    magic: 0xdab5bffa
  },
  bech32: 'bcrt',
  seedsDns: [],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$2);

// source: https://github.com/btcsuite/btcd/blob/6867ff32788a1beb9d148e414d7f84f50958f0d2/chaincfg/params.go#L508
var simnet = Object.assign({}, {
  hashGenesisBlock: 'f67ad7695d9b662a72ff3d8edbbb2de0bfa67b13974bb9910d116d5cbd863e68',
  port: 18555,
  portRpc: 18556,
  protocol: {
    magic: 0x12141c16
  },
  bech32: 'sb',
  seedsDns: [],
  versions: {
    bip32: {
      private: 0x0420b900,
      public: 0x0420bd3a
    },
    bip44: 115,
    private: 0x64,
    public: 0x3f,
    scripthash: 0x7b
  }
}, common$2);

var btc = {
  main: main$2,
  test: test$1,
  regtest: regtest$1,
  simnet
};

/*
  info from:
    https://github.com/BTCGPU/BTCGPU/blob/master/src/chainparams.cpp
*/

var common$3 = {
  name: 'Bitcoin Gold',
  unit: 'BTG'
};

var main$3 = Object.assign({}, {
  hashGenesisBlock: '000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f',
  // nDefaultPort
  port: 8338,
  protocol: {
    // pchMessageStart
    magic: 0x446d47e1 // careful, sent over wire as little endian
  },
  bech32: 'btg',
  // vSeeds
  seedsDns: [
    'eu-dnsseed.bitcoingold-official.org',
    'dnsseed.bitcoingold.org',
    'dnsseed.btcgpu.org'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 156,
    private: 0x80,
    public: 0x26,
    scripthash: 0x17
  }
}, common$3);

var test$2 = Object.assign({}, {
  hashGenesisBlock: '0x00000000e0781ebe24b91eedc293adfea2f557b53ec379e78959de3853e6f9f6',
  port: 18338,
  portRpc: 18332,
  protocol: {
    magic: 0x456e48e2
  },
  bech32: 'tbtg',
  seedsDns: [
    'test-dnsseed.bitcoingold.org',
    'test-dnsseed.btcgpu.org',
    'eu-test-dnsseed.bitcoingold-official.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 156,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$3);

var btg = {
  main: main$3,
  test: test$2
};

/*
  info from:
    https://github.com/c0ban/c0ban/blob/master/src/chainparams.cpp
*/

var common$4 = {
  name: 'c0ban',
  unit: 'RYO'
};

var main$4 = Object.assign({}, {
  hashGenesisBlock: '000000005184ffce04351e687a3965b300ee011d26b2089232cd039273be4a67',
  // nDefaultPort
  port: 3881,
  portRpc: 3882,
  protocol: {
    magic: 0x6e623063 // pchMessageStart
  },
  // vSeeds
  seedsDns: [
    'jp01.dnsseed.c0ban.com',
    'kr01.dnsseed.c0ban.com'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4, // base58Prefixes[EXT_SECRET_KEY]
      public: 0x0488b21e // base58Prefixes[EXT_PUBLIC_KEY]
    },
    // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
    bip44: 88888, // TODO: decide cbn bip44
    private: 0x88, // base58Prefixes[SECRET_KEY]
    public: 0x12, // base58Prefixes[PUBKEY_ADDRESS]
    scripthash: 0x1c // base58Prefixes[SCRIPT_ADDRESS]
  }
}, common$4);

var test$3 = Object.assign({}, {
  hashGenesisBlock: '000000005184ffce04351e687a3965b300ee011d26b2089232cd039273be4a67',
  port: 13881,
  portRpc: 13882,
  protocol: {
    magic: 0x8e828083 // pchMessageStart
  },
  seedsDns: [
  ],
  versions: {
    bip32: {
      private: 0x04388388, // base58Prefixes[EXT_SECRET_KEY]
      public: 0x04588788 // base58Prefixes[EXT_PUBLIC_KEY]
    },
    // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
    bip44: 1,
    private: 0xee, // base58Prefixes[SECRET_KEY]
    public: 0x76, // base58Prefixes[PUBKEY_ADDRESS]
    scripthash: 0xc6 // base58Prefixes[SCRIPT_ADDRESS]
  }
}, common$4);

var regtest$2 = Object.assign({}, {
  hashGenesisBlock: '3249e44acac8fc67e6b94e882525cea6f5a9853e1ff7b4a1d5f470b23ff8ae11',
  port: 23881,
  portRpc: 23882,
  protocol: {
    magic: 0xdab5bffa // pchMessageStart
  },
  seedsDns: [
  ],
  versions: {
    bip32: {
      private: 0x043587cf, // base58Prefixes[EXT_SECRET_KEY]
      public: 0x04358394 // base58Prefixes[EXT_PUBLIC_KEY]
    },
    // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
    bip44: 1,
    private: 0xef, // base58Prefixes[SECRET_KEY]
    public: 0x6f, // base58Prefixes[PUBKEY_ADDRESS]
    scripthash: 0xc4 // base58Prefixes[SCRIPT_ADDRESS]
  }
}, common$4);

var cbn = {
  main: main$4,
  test: test$3,
  regtest: regtest$2
};

var common$5 = {
  name: 'CityCoin',
  isProofOfStake: true
};

var main$5 = Object.assign({}, {
  unit: 'CITY',
  hashGenesisBlock: '00000b0517068e602ed5279c20168cfa1e69884ee4e784909652da34c361bff2',
  port: 4333,
  portRpc: 4334,
  protocol: {
    magic: 0x43545901
  },
  seedsDns: [
    'seed.city-chain.org',
    'seed.city-coin.org',
    'seed.citychain.foundation',
    'seed.liberstad.com'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 1926,
    private: 0xed,
    public: 0x1c,
    scripthash: 0x58
  }
}, common$5);

var test$4 = Object.assign({}, {
  unit: 'TCITY',
  hashGenesisBlock: '00077765f625cc2cb6266544ff7d5a462f25be14ea1116dc2bd2fec17e40a5e3',
  port: 24333,
  portRpc: 24334,
  protocol: {
    magic: 0x43545401
  },
  seedsDns: [
    'testseed.city-chain.org',
    'testseed.city-coin.org',
    'testseed.citychain.foundation'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 1926,
    private: 0xc2,
    public: 0x42,
    scripthash: 0xc4
  }
}, common$5);

var city = {
  main: main$5,
  test: test$4
};

/*
  info from:
    https://github.com/dashpay/dash/blob/master/src/chainparams.cpp
*/

var common$6 = {
  name: 'Dash',
  unit: 'DASH'
};

var main$6 = Object.assign({}, {
  hashGenesisBlock: '00000ffd590b1485b3caadc19b22e6379c733355108f107a430458cdf3407ab6',
  // nDefaultPort
  port: 9999,
  portRpc: 9998,
  protocol: {
    magic: 0xbd6b0cbf // careful, sent over wire as little endian
  },
  // vSeeds
  seedsDns: [
    'dash.org',
    'dnsseed.dash.org',
    'dashdot.io',
    'dnsseed.dashdot.io',
    'masternode.io',
    'dnsseed.masternode.io',
    'dashpay.io',
    'dnsseed.dashpay.io'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 5,
    private: 0xcc,
    public: 0x4c,
    scripthash: 0x10
  }
}, common$6);

var test$5 = Object.assign({}, {
  hashGenesisBlock: '00000bafbc94add76cb75e2ec92894837288a481e5c005f6563d91623bf8bc2c',
  port: 19999,
  portRpc: 19998,
  seedsDns: [
    'dashdot.io',
    'testnet-seed.dashdot.io',
    'masternode.io',
    'test.dnsseed.masternode.io'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x8c,
    scripthash: 0x13
  }
}, common$6);

var dash = {
  main: main$6,
  test: test$5
};

// https://github.com/carsenk/denarius/blob/master/src/main.cpp

var common$7 = {
  name: 'Denarius',
  unit: 'DNR'
};

var main$7 = Object.assign({}, {
  hashGenesisBlock: '00000d5dbbda01621cfc16bbc1f9bf3264d641a5dbf0de89fd0182c2c4828fcd',
  port: 33339,
  portRpc: 32339,
  protocol: {
    magic: 0xb4eff2fa
  },
  seedsDns: [
    'denariusexplorer.org',
    'denarius.host',
    'denarius.tech',
    'denarius.network'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 116,
    private: 0x9e,
    public: 0x1e,
    scripthash: 0x5a
  }
}, common$7);

var test$6 = Object.assign({}, {
  hashGenesisBlock: '000086bfe8264d241f7f8e5393f747784b8ca2aa98bdd066278d590462a4fdb4',
  versions: {
    bip44: 1,
    private: 0x8c,
    public: 0x12,
    scripthash: 0x74
  }
}, common$7);

var dnr = {
  main: main$7,
  test: test$6
};

var common$8 = {
  name: 'Decred',
  unit: 'DCR'
};

// https://github.com/decred/dcrd/blob/ef71103c95cbf77e5a0418e3d413b5906e710b25/chaincfg/params.go
// https://github.com/decred/bitcore/blob/a92381b2b0023b28a1b7eb03e6cb0bfb7800200d/lib/networks.js
var main$8 = Object.assign({}, {
  hashGenesisBlock: '298e5cc3d985bfe7f81dc135f360abe089edd4396b86d2de66b0cef42b21d980',
  port: 9108,
  portRpc: 9109,
  protocol: {
    magic: 0xf900b4d9
  },
  seedsDns: [
    'mainnet-seed.decred.mindcry.org',
    'mainnet-seed.decred.netpurgatory.com',
    'mainnet.decredseed.org',
    'mainnet-seed.decred.org'
  ],
  versions: {
    bip32: {
      private: 0x02fda4e8,
      public: 0x02fda926
    },
    bip44: 42,
    private: 0x22de,
    public: 0x073f,
    scripthash: 0x071a
  }
}, common$8);

var test$7 = Object.assign({}, {
  hashGenesisBlock: '5b7466edf6739adc9b32aaedc54e24bdc59a05f0ced855088835fe3cbe58375f',
  port: 19108,
  portRpc: 19109,
  protocol: {
    magic: 0x48e7a065
  },
  seedsDns: [
    'testnet-seed.decred.mindcry.org',
    'testnet-seed.decred.netpurgatory.org',
    'testnet.decredseed.org',
    'testnet-seed.decred.org'
  ],
  versions: {
    bip32: {
      private: 0x04358397,
      public: 0x043587d1
    },
    bip44: 42,
    private: 0x230e,
    public: 0x0f21,
    scripthash: 0x0efc
  }
}, common$8);

var dcr = {
  main: main$8,
  test: test$7
};

/*
  info from:
    https://github.com/digibyte/digibyte/blob/9e4c0b3ddfd10a7ab852240ff716a7b93af89a07/src/chainparams.cpp
*/

var common$9 = {
  name: 'DigiByte',
  per1: 1e8,
  unit: 'DGB'
};

var main$9 = Object.assign({}, {
  hashGenesisBlock: '000000000019d6689c085ae165831e934ff763ae46a2a6c172b3f1b60a8ce26f',
  // nDefaultPort
  port: 12024,
  portRpc: 14022,
  protocol: {
    // pchMessageStart
    magic: 0xfac3b6da // careful, sent over wire as little endian
  },
  bech32: 'dgb',
  // vSeeds
  seedsDns: [
    'seed.digibyte.io',
    'digiexplorer.info',
    'digihash.co'
  ],
  // base58Prefixes
  versions: {
    bip44: 0x80000014,
    private: 0x80,
    public: 0x1e,
    scripthash: 0x3f, // new 'S' prefix
    scripthash2: 0x05 // old '3' prefix
  }
}, common$9);

var dgb = { main: main$9 };

// https://github.com/dogecoin/dogecoin/blob/master/src/chainparams.cpp

var common$a = {
  name: 'Dogecoin',
  unit: 'DOGE'
};

var main$a = Object.assign({}, {
  hashGenesisBlock: '1a91e3dace36e2be3bf030a65679fe821aa1d6ef92e7c9902eb318182c355691',
  port: 22556,
  protocol: {
    magic: 0xc0c0c0c0
  },
  seedsDns: [
    'seed.multidoge.org',
    'seed2.multidoge.org'
  ],
  versions: {
    bip32: {
      private: 0x02fac398,
      public: 0x02facafd
    },
    bip44: 3,
    private: 0x9e,
    public: 0x1e,
    scripthash: 0x16
  }
}, common$a);

var test$8 = Object.assign({}, {
  hashGenesisBlock: 'bb0a78264637406b6360aad926284d544d7049f45189db5664f3c4d07350559e',
  port: 44556,
  protocol: {
    magic: 0xfcc1b7dc
  },
  seedsDns: [
    'testseed.jrn.me.uk'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xf1,
    public: 0x71,
    scripthash: 0xc4
  }
}, common$a);

var doge = {
  main: main$a,
  test: test$8
};

/*
  info from:
    https://github.com/bitcoin/bitcoin/blob/master/src/chainparams.cpp
*/

var common$b = {
  name: 'Groestlcoin',
  per1: 1e8,
  unit: 'GRS',
  messagePrefix: '\x1CGroestlCoin Signed Message:\n'
};

var main$b = Object.assign({}, {
  hashGenesisBlock: '00000ac5927c594d49cc0bdb81759d0da8297eb614683d3acb62f0703b639023',
  // nDefaultPort
  port: 1331,
  portRpc: 1441,
  protocol: {
    // pchMessageStart
    magic: 0xd4b4bef9 // careful, sent over wire as little endian
  },
  bech32: 'grs',
  // vSeeds
  seedsDns: [
    'dnsseed1.groestlcoin.org',
    'dnsseed2.groestlcoin.org',
    'dnsseed3.groestlcoin.org',
    'dnsseed4.groestlcoin.org'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 17,
    private: 0x80,
    public: 0x24,
    scripthash: 0x05
  }
}, common$b);

var test$9 = Object.assign({}, {
  hashGenesisBlock: '0x000000ffbb50fc9898cdd36ec163e6ba23230164c0052a28876255b7dcf2cd36',
  port: 17777,
  portRpc: 17766,
  protocol: {
    magic: 0x0709110b
  },
  bech32: 'tgrs',
  seedsDns: [
    'testnet-seed1.groestlcoin.org',
    'testnet-seed2.groestlcoin.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$b);

var regtest$3 = Object.assign({}, {
  hashGenesisBlock: '0x000000ffbb50fc9898cdd36ec163e6ba23230164c0052a28876255b7dcf2cd36',
  port: 18888,
  portRpc: 18443,
  protocol: {
    magic: 0xdab5bffa
  },
  bech32: 'grsrt',
  seedsDns: [],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$b);

var grs = {
  main: main$b,
  test: test$9,
  regtest: regtest$3
};

// https://github.com/litecoin-project/litecoin/blob/master-0.10/src/chainparams.cpp

var common$c = {
  name: 'Litecoin',
  unit: 'LTC'
};

var main$c = Object.assign({}, {
  hashGenesisBlock: '12a765e31ffd4059bada1e25190f6e98c99d9714d334efa41a195a7e7e04bfe2',
  port: 9333,
  protocol: {
    magic: 0xdbb6c0fb
  },
  bech32: 'ltc',
  seedsDns: [
    'dnsseed.litecointools.com',
    'dnsseed.litecoinpool.org',
    'dnsseed.ltc.xurious.com',
    'dnsseed.koin-project.com',
    'dnsseed.weminemnc.com'
  ],
  versions: {
    bip32: {
      private: 0x019d9cfe,
      public: 0x019da462
    },
    bip44: 2,
    private: 0xb0,
    public: 0x30,
    scripthash: 0x32,
    scripthash2: 0x05 // old '3' prefix. available for backward compatibility.
  }
}, common$c);

var test$a = Object.assign({}, {
  hashGenesisBlock: 'f5ae71e26c74beacc88382716aced69cddf3dffff24f384e1808905e0188f68f',
  bech32: 'tltc',
  versions: {
    bip32: {
      private: 0x0436ef7d,
      public: 0x0436f6e1
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0x3a,
    scripthash2: 0xc4
  }
}, common$c);

var ltc = {
  main: main$c,
  test: test$a
};

// https://github.com/viacoin/viacoin/blob/master/src/chainparams.cpp

var common$d = {
  name: 'Viacoin',
  unit: 'VIA'
};

var main$d = Object.assign({}, {
  hashGenesisBlock: '4e9b54001f9976049830128ec0331515eaabe35a70970d79971da1539a400ba1',
  port: 5223,
  protocol: {
    magic: 0xcbc6680f
  },
  seedsDns: [
    'seed.viacoin.net',
    'viaseeder.barbatos.fr',
    'mainnet.viacoin.net'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 14,
    private: 0xc7,
    public: 0x47,
    scripthash: 0x21
  }
}, common$d);

var test$b = Object.assign({}, {
  hashGenesisBlock: '770aa712aa08fdcbdecc1c8df1b3e2d4e17a7cf6e63a28b785b32e74c96cb27d',
  port: 25223,
  protocol: {
    magic: 0x92efc5a9
  },
  seedsDns: [
    'testnet.viacoin.net',
    'seed-testnet.viacoin.net'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xff,
    public: 0x7f,
    scripthash: 0xc4
  }
}, common$d);

var via = {
  main: main$d,
  test: test$b
};

// https://github.com/monacoinproject/monacoin/blob/master-0.13/src/chainparams.cpp

var common$e = {
  name: 'Monacoin',
  unit: 'MONA'
};

var main$e = Object.assign({}, {
  hashGenesisBlock: 'ff9f1c0116d19de7c9963845e129f9ed1bfc0b376eb54fd7afa42e0d418c8bb6',
  port: 9401,
  portRpc: 9402,
  protocol: {
    magic: 0xdbb6c0fb
  },
  bech32: 'mona',
  seedsDns: [
    'dnsseed.monacoin.org'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 22,
    private: 0xb0,
    private2: 0xb2, // old wif
    public: 0x32,
    scripthash: 0x37,
    scripthash2: 0x05 // old '3' prefix. available for backward compatibility.
  }
}, common$e);

var test$c = Object.assign({}, {
  hashGenesisBlock: 'a2b106ceba3be0c6d097b2a6a6aacf9d638ba8258ae478158f449c321061e0b2',
  port: 19403,
  portRpc: 19402,
  protocol: {
    magic: 0xf1c8d2fd
  },
  bech32: 'tmona',
  seedsDns: [
    'testnet-dnsseed.monacoin.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0x75,
    scripthash2: 0xc4
  }
}, common$e);

var mona = {
  main: main$e,
  test: test$c
};

var common$f = {
  name: 'NuBits',
  per1: 1e6,
  unit: 'NBT'
};

var main$f = Object.assign({}, {
  hashGenesisBlock: '000003cc2da5a0a289ad0a590c20a8b975219ddc1204efd169e947dd4cbad73f',
  // nDefaultPort
  port: 7890,
  portRpc: 14002,
  protocol: {
    // pchMessageStart
    magic: 0xd9b4bef9 // careful, sent over wire as little endian
  },
  // vSeeds
  seedsDns: [
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 12,
    private: 0x96,
    public: 0x19,
    scripthash: 0x1a
  }
}, common$f);

var nbt = {
  main: main$f
};

var common$g = {
  name: 'Namecoin',
  unit: 'NMC'
};

var main$g = Object.assign({}, {
  hashGenesisBlock: '000000000062b72c5e2ceb45fbc8587e807c155b0da735e6483dfba2f0a9c770',
  versions: {
    bip44: 7,
    private: 0xb4,
    public: 0x34,
    scripthash: 0x05
  }
}, common$g);

var nmc = {
  main: main$g,
  test: null
};

// https://github.com/peercoin/peercoin/tree/v0.7.0ppc/src
// https://github.com/peercoin/peercoin/blob/v0.7.0ppc/src/${filename}

var common$h = {
  name: 'Peercoin',
  per1: 1e6, // util.h:40
  unit: 'PPC',
  messagePrefix: '\x18Peercoin Signed Message:\n' // main.cpp:77
};

var main$h = Object.assign({}, {
  hashGenesisBlock: '0000000032fe677166d54963b62a4677d8957e87c508eaa4fd7eb1c880cd27e3', // main.h:84
  // nDefaultPort
  port: 9901, // protocol.h:18
  portRpc: 9902, // protocol.h:19
  protocol: {
    // pchMessageStart
    magic: 0xe5e9e8e6 // careful, sent over wire as little endian protocol.cpp:31
  },
  // vSeeds
  seedsDns: [
    // net.cpp:1209
    'seed.peercoin.net',
    'seed2.peercoin.net',
    'seed.peercoin-library.org',
    'ppcseed.ns.7server.net'
  ],
  versions: {
    // not implemented in Peercoin <= v0.7.x nodes, only 3rd party wallets
    // https://github.com/jmacwhyte/recovery-phrase-recovery/blob/52073aba08e9d01032c0b5aff8c682911fe2e5fc/js/bitcoinjs-extensions.js#L58
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 6, // https://github.com/satoshilabs/slips/blob/master/slip-0044.md
    private: 0xb7, // base58.h:402 ; 128 + PUBKEY_ADDRESS
    public: 0x37, // base58.h:276
    scripthash: 0x75 // base58.h:277
  }
}, common$h);

var test$d = Object.assign({}, {
  hashGenesisBlock: '00000001f757bb737f6596503e17cd17b0658ce630cc727c0cca81aec47c9f06',
  port: 9903,
  portRpc: 9904,
  protocol: {
    magic: 0xefc0f2cb
  },
  seedsDns: [
    'tseed.peercoin.net',
    'tseed2.peercoin.net',
    'tseed.peercoin-library.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$h);

var ppc = {
  main: main$h,
  test: test$d
};

/*
  info from:
    https://github.com/qtumproject/qtum/blob/master/src/chainparams.cpp
*/

var common$i = {
  name: 'Qtum',
  unit: 'QTUM'
};

var main$i = Object.assign({}, {
  hashGenesisBlock: '000075aef83cf2853580f8ae8ce6f8c3096cfa21d98334d6e3f95e5582ed986c',
  // nDefaultPort
  port: 3888,
  protocol: {
    // pchMessageStart
    magic: 0xd3a6cff1 // careful, sent over wire as little endian
  },
  bech32: 'qc',
  // vSeeds
  seedsDns: [
    'qtum3.dynu.net'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 2301,
    private: 0x80,
    public: 0x3A,
    scripthash: 0x32
  }
}, common$i);

var qtum = {
  main: main$i
};

// https://github.com/RavenProject/Ravencoin/blob/master/src/chainparams.cpp

var common$j = {
  name: 'Ravencoin',
  unit: 'RVN'
};

var main$j = Object.assign({}, {
  hashGenesisBlock: '0000006b444bc2f2ffe627be9d9e7e7a0730000870ef6eb6da46c8eae389df90',
  port: 8767,
  protocol: {
    magic: 0x4e564152
  },
  bech32: 'rc',
  seedsDns: [
    'seed-raven.bitactivate.com',
    'seed-raven.ravencoin.com',
    'seed-raven.ravencoin.org'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 175,
    private: 0x80,
    public: 0x3c,
    scripthash: 0x7a
  }
}, common$j);

var test$e = Object.assign({}, {
  hashGenesisBlock: '000000ecfc5e6324a079542221d00e10362bdc894d56500c414060eea8a3ad5a',
  port: 18770,
  protocol: {
    magic: 0x544e5652
  },
  bech32: 'tr',
  seedsDns: [
    'seed-testnet-raven.bitactivate.com',
    'seed-testnet-raven.ravencoin.com',
    'seed-testnet-raven.ravencoin.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$j);

var rvn = {
  main: main$j,
  test: test$e
};

var common$k = {
  name: 'ReddCoin',
  unit: 'RDD'
};

var main$k = Object.assign({}, {
  hashGenesisBlock: 'b868e0d95a3c3c0e0dadc67ee587aaf9dc8acbf99e3b4b3110fad4eb74c1decc',
  versions: {
    bip44: 4,
    private: 0xbd,
    public: 0x3d,
    scripthash: 0x05
  }
}, common$k);

var test$f = Object.assign({}, {
  hashGenesisBlock: 'a12ac9bd4cd26262c53a6277aafc61fe9dfe1e2b05eaa1ca148a5be8b394e35a',
  versions: {
    bip44: 1,
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$k);

var rdd = {
  main: main$k,
  test: test$f
};

/*
  info from:
    https://github.com/vertcoin/vertcoin/blob/master/src/chainparams.cpp
*/

var common$l = {
  name: 'Vertcoin',
  unit: 'VTC'
};

var main$l = Object.assign({}, {
  hashGenesisBlock: '4d96a915f49d40b1e5c2844d1ee2dccb90013a990ccea12c492d22110489f0c4',
  // nDefaultPort
  port: 5889,
  protocol: {
    // pchMessageStart
    magic: 0xdab5bffa // careful, sent over wire as little endian
  },
  bech32: 'vtc',
  // vSeeds
  seedsDns: [
    'useast1.vtconline.org',
    'vtc.gertjaap.org',
    'seed.vtc.bryangoodson.org',
    'dnsseed.pknight.ca',
    'seed.orderofthetaco.org',
    'seed.alexturek.org',
    'vertcoin.mbl.cash'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 28,
    private: 0x80,
    public: 0x47,
    scripthash: 0x05
  }
}, common$l);

var test$g = Object.assign({}, {
  hashGenesisBlock: 'cee8f24feb7a64c8f07916976aa4855decac79b6741a8ec2e32e2747497ad2c9',
  port: 15889,
  // portRpc: 18332,
  protocol: {
    magic: 0x74726576
  },
  bech32: 'tvtc',
  seedsDns: [
    'jlovejoy.mit.edu',
    'gertjaap.ddns.net',
    'fr1.vtconline.org',
    'tvtc.vertcoin.org'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    private: 0xef,
    public: 0x4a,
    scripthash: 0xc4
  }
}, common$l);

var regtest$4 = Object.assign({}, {
  hashGenesisBlock: '0f9188f13cb7b2c71f2a335e3a4fc328bf5beb436012afca590b1a11466e2206',
  port: 18444,
  // portRpc: 18332,
  protocol: {
    magic: 0xdab5bffa
  },
  seedsDns: [],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    private: 0xef,
    public: 0x6f,
    scripthash: 0xc4
  }
}, common$l);

var vtc = {
  main: main$l,
  test: test$g,
  regtest: regtest$4
};

var common$m = {
  name: 'x42',
  isProofOfStake: true
};

var main$m = Object.assign({}, {
  unit: 'x42',
  hashGenesisBlock: '04ffe583707a96c1c2eb54af33a4b1dc6d9d8e09fea8c9a7b097ba88f0cb64c4',
  port: 52342,
  portRpc: 52343,
  protocol: {
    magic: 0x3526642
  },
  seedsDns: [
    'mainnet1.x42seed.host',
    'mainnetnode1.x42seed.host',
    'tech.x42.cloud',
    'x42.seed.blockcore.net'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 424242,
    private: 0xcb,
    public: 0x4b,
    scripthash: 0x7d
  }
}, common$m);

var test$h = Object.assign({}, {
  unit: 'Tx42',
  hashGenesisBlock: 'a92bf124a1e6f237015440d5f1e1999bdef8e321f2d3fdc367eb2f7733b17854',
  port: 62342,
  portRpc: 62343,
  protocol: {
    magic: 0x4526642
  },
  seedsDns: [
    'testnet1.x42seed.host'
  ],
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 424242,
    private: 0xc1,
    public: 0x41,
    scripthash: 0xc4
  }
}, common$m);

var x42 = {
  main: main$m,
  test: test$h
};

/*
  info from:
    https://github.com/zcash/zcash/blob/v1.0.12/src/chainparamsbase.cpp
    https://github.com/zcash/zcash/blob/v1.0.12/src/chainparams.cpp
*/

var common$n = {
  name: 'Zcash',
  unit: 'ZEC'
};

var main$n = Object.assign({}, {
  hashGenesisBlock: '00040fe8ec8471911baa1db1266ea15dd06b4a8a5c453883c000b031973dce08',
  // nDefaultPort
  port: 8233,
  portRpc: 8232,
  protocol: {
    // pchMessageStart
    magic: 0x6427e924 // careful, sent over wire as little endian
  },
  // vSeeds
  seedsDns: [
    'dnsseed.z.cash',
    'dnsseed.str4d.xyz',
    'dnsseed.znodes.org'
  ],
  // base58Prefixes
  versions: {
    bip32: {
      private: 0x0488ade4,
      public: 0x0488b21e
    },
    bip44: 133,
    private: 0x80,
    public: 0x1cb8,
    scripthash: 0x1cbd
  }
}, common$n);

var test$i = Object.assign({}, {
  hashGenesisBlock: '0x05a60a92d99d85997cce3b87616c089f6124d7342af37106edc76126334a2c38',
  port: 18233,
  portRpc: 18232,
  protocol: {
    magic: 0xbff91afa
  },
  seedsDns: [
    'dnsseed.testnet.z.cash'
  ],
  versions: {
    bip32: {
      private: 0x04358394,
      public: 0x043587cf
    },
    bip44: 133,
    private: 0xef,
    public: 0x1d25,
    scripthash: 0x1cba
  }
}, common$n);

var zec = {
  main: main$n,
  test: test$i
};

var Buffer$1 = safeBuffer.Buffer;

// annoyingly, this is for browserify
var coins = [
  bch,
  blk,
  btc,
  btg,
  cbn,
  city,
  dash,
  dnr,
  dcr,
  dgb,
  doge,
  grs,
  ltc,
  via,
  mona,
  nbt,
  nmc,
  ppc,
  qtum,
  rvn,
  rdd,
  vtc,
  x42,
  zec
];

var supportedCoins = {};

coins.forEach(function (coin) {
  var unit = coin.main.unit.toLowerCase();
  var name = coin.main.name.toLowerCase();

  coin.main.testnet = false;
  coin.main.toBitcoinJS = toBitcoinJS.bind(coin.main);
  coin.main.toBitcore = toBitcore.bind(coin.main);
  supportedCoins[unit] = coin.main;
  supportedCoins[name] = coin.main;

  if (coin.test) {
    coin.test.testnet = true;
    coin.test.toBitcoinJS = toBitcoinJS.bind(coin.test);
    coin.test.toBitcore = toBitcore.bind(coin.test);
    supportedCoins[unit + '-test'] = coin.test;
    supportedCoins[name + '-test'] = coin.test;
  }

  if (coin.regtest) {
    coin.regtest.testnet = true;
    coin.regtest.toBitcoinJS = toBitcoinJS.bind(coin.regtest);
    coin.regtest.toBitcore = toBitcore.bind(coin.regtest);
    supportedCoins[unit + '-regtest'] = coin.regtest;
    supportedCoins[name + '-regtest'] = coin.regtest;
  }

  if (coin.simnet) {
    coin.simnet.testnet = true;
    coin.simnet.toBitcoinJS = toBitcoinJS.bind(coin.simnet);
    coin.simnet.toBitcore = toBitcore.bind(coin.simnet);
    supportedCoins[unit + '-simnet'] = coin.simnet;
    supportedCoins[name + '-simnet'] = coin.simnet;
  }
});

function coininfo (input) {
  var coin = input.toLowerCase();

  if (!(coin in supportedCoins)) {
    return null
  } else {
    return supportedCoins[coin]
  }
}

coins.forEach(function (coin) {
  coininfo[coin.main.name.toLowerCase()] = coin;
});

function toBitcoinJS () {
  return Object.assign({}, this, {
    messagePrefix: this.messagePrefix || ('\x19' + this.name + ' Signed Message:\n'),
    bech32: this.bech32,
    bip32: {
      public: (this.versions.bip32 || {}).public,
      private: (this.versions.bip32 || {}).private
    },
    pubKeyHash: this.versions.public,
    scriptHash: this.versions.scripthash,
    wif: this.versions.private,
    dustThreshold: null // TODO
  })
}

function toBitcore () {
  // reverse magic
  var nm = Buffer$1.allocUnsafe(4);
  nm.writeUInt32BE(this.protocol ? this.protocol.magic : 0, 0);
  nm = nm.readUInt32LE(0);

  return Object.assign({}, this, {
    name: this.testnet ? 'testnet' : 'livenet',
    alias: this.testnet ? 'testnet' : 'mainnet',
    pubkeyhash: this.versions.public,
    privatekey: this.versions.private,
    scripthash: this.versions.scripthash,
    xpubkey: (this.versions.bip32 || {}).public,
    xprivkey: (this.versions.bip32 || {}).private,
    networkMagic: nm,
    port: this.port,
    dnsSeeds: this.seedsDns || []
  })
}

var coininfo_1 = coininfo;

/** Provider that interacts with the wallet. */
class BlockcoreProvider {
    /** Returns network definition from local package, no external requests. */
    getNetwork(network) {
        return coininfo_1(network);
    }
    // eip-1193: https://github.com/ethereum/EIPs/blob/master/EIPS/eip-1193.md
    /** Use this method to send a request directly to the wallet extension. */
    async request(args) {
        const gthis = globalThis;
        const blockcore = gthis.blockcore;
        if (!blockcore) {
            alert('The Blockcore provider is not available. Unable to continue.');
            return;
        }
        let result;
        try {
            result = await blockcore.request(args);
        }
        catch (err) {
            console.error(err);
            result = 'Error: ' + err.message;
        }
        return result;
    }
}

var __classPrivateFieldGet = (undefined && undefined.__classPrivateFieldGet) || function (receiver, state, kind, f) {
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
    return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
};
var __classPrivateFieldSet = (undefined && undefined.__classPrivateFieldSet) || function (receiver, state, value, kind, f) {
    if (kind === "m") throw new TypeError("Private method is not writable");
    if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
    if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
    return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
};
var _IndexerProvider_network;
class IndexerProvider {
    constructor() {
        Object.defineProperty(this, "dns", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: void 0
        });
        _IndexerProvider_network.set(this, 'STRAX'); // Should we default to BTC?
        Object.defineProperty(this, "currentServices", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: []
        });
        this.dns = new BlockcoreDns();
    }
    get network() {
        return __classPrivateFieldGet(this, _IndexerProvider_network, "f");
    }
    setNetwork(network) {
        __classPrivateFieldSet(this, _IndexerProvider_network, network, "f");
        this.filterServices();
    }
    filterServices() {
        this.currentServices = this.dns.getOnlineServicesByNetwork(this.network);
    }
    /** Attempts to load the latest status of all services from all known nameservers. */
    async load() {
        await this.dns.load();
        this.filterServices();
    }
    on(event, callback) {
        console.log(event, callback);
        // "accountsChanged"
        // "chainChanged"
        // "networkChanged"
    }
    getRandomInt(max) {
        return Math.floor(Math.random() * max);
    }
    getUrl() {
        // TODO: This can be simplified, I'm just too tired to refactor right now.
        if (this.currentServices.length > 1) {
            const serviceIndex = this.getRandomInt(this.currentServices.length);
            return `https://${this.currentServices[serviceIndex]?.domain}`;
        }
        else if (this.currentServices.length == 1) {
            return `https://${this.currentServices[0]?.domain}`;
        }
        else {
            return undefined;
        }
    }
    //** Returns the result from the officially hosted list of Blockcore supported chains. */
    getNetworks() {
        return WebRequest.fetchJson('https://chains.blockcore.net/CHAINS.json');
    }
    getSupply() {
        return WebRequest.fetchJson(this.getUrl() + '/api/insight/supply');
    }
    async getCirculatingSupply() {
        return WebRequest.fetchText(this.getUrl() + '/api/insight/supply/circulating');
    }
    async getTotalSupply() {
        return WebRequest.fetchText(this.getUrl() + '/api/insight/supply/total');
    }
    async getEstimateRewards() {
        return WebRequest.fetchText(this.getUrl() + '/api/insight/rewards');
    }
    async getWallets() {
        return WebRequest.fetchJson(this.getUrl() + '/api/insight/wallets');
    }
    async getRichList() {
        return WebRequest.fetchJson(this.getUrl() + '/api/insight/richlist');
    }
    async getAddress(address) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/address/${address}`);
    }
    async getAddressTransactions(address) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/address/${address}/transactions`);
    }
    async getAddressUnconfirmedTransactions(address) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/address/${address}/transactions/unconfirmed`);
    }
    async getAddressSpentTransactions(address) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/address/${address}/transactions/spent`);
    }
    async getAddressUnspentTransactions(address) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/address/${address}/transactions/unspent`);
    }
    async getMempoolTransactions() {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/mempool/transactions`);
    }
    async getMempoolTransactionsCount() {
        return WebRequest.fetchText(`${this.getUrl()}/api/query/mempool/transactions/count`);
    }
    async getTransactionById(id) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/transaction/${id}`);
    }
    async getBlock() {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block`);
    }
    async getBlockTransactionsByHash(hash) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block/${hash}/transactions`);
    }
    async getBlockByHash(hash) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block/${hash}`);
    }
    async getBlockByIndex(index) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block/index/${index}`);
    }
    async getBlockTransactionsByIndex(index) {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block/index/${index}/transactions`);
    }
    async getLatestBlock() {
        return WebRequest.fetchJson(`${this.getUrl()}/api/query/block/latest`);
    }
}
_IndexerProvider_network = new WeakMap();

class IdentityProvider {
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-did-supported-methods.md
    /** This method can be used by clients to become aware of the DID methods supported by a wallet. */
    async supportedMethods() {
        return ['did:is', 'did:jwk', 'did:key'];
    }
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-did-authn.md
    /** Initiates DID-based passwordless registration / login flows */
    async authn() {
        throw Error('Not implemented.');
    }
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-did-request.md
    async request() {
        throw Error('Not implemented.');
    }
}

class VerifiableCredentialProvider {
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-vc-apply.md
    /** Initiates a credential application flow in the wallet using a Credential Manifest */
    async apply() {
        throw Error('Not implemented.');
    }
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-vc-deliver.md
    /** Delivers Verifiable Credentials to a wallet */
    async deliver() {
        throw Error('Not implemented.');
    }
    // https://github.com/TBD54566975/janky-wallet/blob/main/rfc/web5-vc-request.md
    /** Requests Verifiable Credentials from the wallet using Presentation Exchange */
    async request() {
        throw Error('Not implemented.');
    }
}

class WebProvider {
    constructor(indexer = new IndexerProvider(), provider = new BlockcoreProvider(), did = new IdentityProvider(), vc = new VerifiableCredentialProvider()) {
        Object.defineProperty(this, "indexer", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: indexer
        });
        Object.defineProperty(this, "provider", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: provider
        });
        Object.defineProperty(this, "did", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: did
        });
        Object.defineProperty(this, "vc", {
            enumerable: true,
            configurable: true,
            writable: true,
            value: vc
        });
    }
    static async Create(indexer) {
        // Create and load all known services from all name servers.
        if (!indexer) {
            indexer = new IndexerProvider();
            await indexer.load();
        }
        const provider = new BlockcoreProvider();
        const webProvider = new WebProvider(indexer, provider);
        return webProvider;
    }
    setNetwork(network) {
        this.indexer.setNetwork(network);
    }
    async request(args) {
        return this.provider.request(args);
    }
    /** Returns network definition from local package, no external requests. */
    getNetwork(network) {
        return this.provider.getNetwork(network);
    }
}

export { WebProvider };
