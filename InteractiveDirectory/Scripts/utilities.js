// function: Sort
// author: WSC
// purpose: Array/JSON object sort.
/* usage: 
[3,2,4,5].sort(new sort().by(<accessor>).desc().thenBy(<accessor>).thenBy(<accessor>).asc().build());
<accessor> signature: function(item) { return item.<property to sort on>; }
example:
[
{firstName:"John", lastName: "Johnson", order:1},
{firstName:"Jake", lastName: "Jennison", order:1},
{firstName:"Jenna", lastName: "Jennison", order:2},
{firstName:"Jack", lastName: "Johnson", order:1}
]
.sort(
new sort()
.by(function(item){ return item.order; }).desc()
.thenBy(function(item){ return item.lastName; }).asc()
.thenBy(function(item){ return item.firstName; }) // implied ascending
.build() // returns compareFunction for the Array.prototype.sort function
);

results in:
[
{firstName:"Jenna", lastName: "Jennison", order:2},
{firstName:"Jake", lastName: "Jennison", order:1},
{firstName:"Jack", lastName: "Johnson", order:1},
{firstName:"John", lastName: "Johnson", order:1}
]
*/
function sort() {
    var self = this;

    var accessors = [];
    var currentAccessor = {};
    var compareAsc = function (l, r) { return l < r ? -1 : (l > r ? 1 : 0); };
    var compareDesc = function (l, r) { return compareAsc(r, l); };

    self.by = function (accessor) {
        accessors.push({ accessor: accessor, compare: compareAsc });
        return self;
    };
    self.thenBy = self.by;
    self.isAsc = function (isAscending) {
        return isAscending ? self.asc() : self.desc();
    };
    self.asc = function () {
        accessors[accessors.length - 1].compare = compareAsc;
        return self;
    };
    self.desc = function () {
        accessors[accessors.length - 1].compare = compareDesc;
        return self;
    };
    self.build = function () {
        // if no accessors are present, default to a basic item ascending sort
        if (accessors.length == 0) self.by(function (item) { return item; });

        return function (l, r) {
            var result = accessors.reduce(function (p, c) {
                return (p == 0) ? c.compare(c.accessor(l), c.accessor(r)) : p;
            }, 0);

            return result;
        }
    };
};
