### Project Full Stack by Điền Nguyễn

Hướng dẫn sinh database:

`Tools -> NuGet Package Manager -> Package Manager Console`

Nhập vào:

> Add-Migration Initial

> Add-Migration InitialOrder

> Add-Migration StatusOrder

> Add-Migration ExtendIdentity

> Add-Migration ExtendIdentityUser

> Update-Database

Kiểm tra thư mục `Migrations` sẽ có các file như [ảnh](https://imgur.com/a/TsuOC72/)

Kiểm tra `database trong Sql Server` xem đã có chưa

Nếu đã có tiến hành run (nếu không hiện nút run thì do chưa chọn `Solution "FashionShop"` bên phải

Kiểm tra dịch vụ API hoạt động chưa: 

`https://localhost:7264/swagger/index.html` (7264 có thể thay đổi tùy máy)
