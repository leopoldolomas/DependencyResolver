//--------------------------------------------------------------- @License begins
// "DependencyResolver"
// 2015 Leopoldo Lomas Flores. Torreon, Coahuila. MEXICO
// leopoldolomas [at] gmail
// www.leopoldolomas.info
// 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or distribute this
// software, either in source code form or as a compiled binary, for any purpose,
// commercial or non-commercial, and by any means.
// 
// In jurisdictions that recognize copyright laws, the author or authors of this
// software dedicate any and all copyright interest in the software to the public
// domain. We make this dedication for the benefit of the public at large and to
// the detriment of our heirs and successors. We intend this dedication to be
// an overt act of relinquishment in perpetuity of all present and future
// rights to this software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//--------------------------------------------------------------- @License ends

namespace Leos.DependencyResolver
{
    public class Bindable
    {
        public DependencyInfo DependencyInfo { get; set; }
    }

    public class Bind<T> : Bindable
    {
        public To<K> To<K>()
        {
            this.DependencyInfo.DependencyType = typeof(T);

            var to = new To<K>();
            to.DependencyInfo = this.DependencyInfo;
            to.DependencyInfo.ServiceType = typeof(K);

            return to;
        }
    }

    public class To<T> : Bindable
    {
        public When<K> When<K>()
        {
            this.DependencyInfo.EnumType = typeof(K);

            var when = new When<K>();
            when.DependencyInfo = this.DependencyInfo;

            return when;
        }
    }

    public class When<T> : Bindable
    {
        public void IsEqualTo(int value)
        {
            this.DependencyInfo.EnumType = typeof(T);
            this.DependencyInfo.EnumValue = value;
        }
    }
}
